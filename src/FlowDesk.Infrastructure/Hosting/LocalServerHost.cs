using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using FlowDesk.Infrastructure.Data;
using Microsoft.AspNetCore.SignalR;

namespace FlowDesk.Infrastructure.Hosting;

public class LocalServerHost
{
    private WebApplication? _app;
    private CancellationTokenSource? _cts;

    public bool IsRunning => _app != null;
    public static event Action<string>? OnMemberDisconnected;
    public static event Action<FlowDesk.Core.Models.TaskComment>? OnLocalApiTaskCommentCreated;
    public static event Action<FlowDesk.Core.Models.TaskItem>? OnLocalApiTaskUpdated;

    public async Task StartAsync(bool allowLanAccess)
    {
        if (_app != null)
        {
            await StopAsync();
        }

        var builder = WebApplication.CreateBuilder();

        // Clear transient members on server start
        using (var db = new FlowDeskDbContext())
        {
            var nonOwners = db.LocalUsers.Where(u => u.Role != "Owner").ToList();
            if (nonOwners.Any())
            {
                db.LocalUsers.RemoveRange(nonOwners);
                db.SaveChanges();
            }
        }

        // Configure URLs
        var hostUrl = allowLanAccess ? "http://0.0.0.0:5050" : "http://localhost:5050";
        builder.WebHost.UseUrls(hostUrl);

        // Add services
        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
            });
        });
        builder.Services.AddSignalR(options =>
        {
            options.KeepAliveInterval = TimeSpan.FromSeconds(2);
            options.ClientTimeoutInterval = TimeSpan.FromSeconds(5);
        });

        _app = builder.Build();

        _app.UseCors();
        
        _app.MapHub<WorkspaceHub>("/hubs/workspace");

        // Minimal APIs
        MapEndpoints(_app);

        _cts = new CancellationTokenSource();
        _ = _app.RunAsync(_cts.Token);
    }

    public static IHubContext<WorkspaceHub>? HubContext { get; private set; }

    public async Task StopAsync()
    {
        if (_app != null && _cts != null)
        {
            _cts.Cancel();
            await _app.StopAsync();
            await _app.DisposeAsync();
            _app = null;
            _cts.Dispose();
            _cts = null;
            HubContext = null;
        }
    }

    private void MapEndpoints(WebApplication app)
    {
        HubContext = app.Services.GetService<IHubContext<WorkspaceHub>>();
        // Middleware for Join Code validation
        app.Use(async (context, next) =>
        {
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                var queryCode = context.Request.Query["code"].ToString();
                
                // Allow Bearer token as Join Code
                var authHeader = context.Request.Headers["Authorization"].ToString();
                if (string.IsNullOrEmpty(queryCode) && authHeader.StartsWith("Bearer "))
                {
                    queryCode = authHeader.Substring("Bearer ".Length).Trim();
                }
                
                using var db = new FlowDeskDbContext();
                var workspace = db.Workspaces.FirstOrDefault();
                
                if (workspace == null || (workspace.JoinCode != null && workspace.JoinCode != queryCode))
                {
                    context.Response.StatusCode = 401; // Unauthorized
                    await context.Response.WriteAsJsonAsync(new { Error = "Invalid or missing Join Code" });
                    return;
                }
            }
            await next(context);
        });

        // Endpoints
        app.MapGet("/api/workspace", () =>
        {
            using var db = new FlowDeskDbContext();
            var workspace = db.Workspaces.FirstOrDefault();
            if (workspace == null) return Results.NotFound();

            return Results.Ok(new
            {
                workspace.Id,
                workspace.Name,
                workspace.HostName,
                workspace.JoinCode
            });
        });

        app.MapGet("/api/projects", () =>
        {
            using var db = new FlowDeskDbContext();
            var projects = db.Projects.ToList();
            return Results.Ok(projects.Select(p => new
            {
                p.Id,
                p.Name,
                p.UpdatedAt
            }));
        });
        app.MapGet("/api/projects/{id}", (Guid id) => {
            using var db = new FlowDeskDbContext();
            var p = db.Projects.FirstOrDefault(x => x.Id == id);
            return p != null ? Results.Ok(p) : Results.NotFound();
        });
        app.MapPost("/api/projects", async (FlowDesk.Core.Models.Project project) => {
            using var db = new FlowDeskDbContext();
            db.Projects.Add(project);
            await db.SaveChangesAsync();
            return Results.Ok(project);
        });
        app.MapPut("/api/projects/{id}", async (Guid id, FlowDesk.Core.Models.Project project) => {
            using var db = new FlowDeskDbContext();
            var existing = await db.Projects.FindAsync(id);
            if (existing != null)
            {
                existing.Name = project.Name;
                existing.Description = project.Description;
                existing.UpdatedAt = DateTime.UtcNow;
                await db.SaveChangesAsync();
            }
            return Results.Ok();
        });
        app.MapDelete("/api/projects/{id}", async (Guid id) => {
            using var db = new FlowDeskDbContext();
            var p = await db.Projects.FindAsync(id);
            if(p!=null) { db.Projects.Remove(p); await db.SaveChangesAsync(); }
            return Results.Ok();
        });

        app.MapGet("/api/tasks", () =>
        {
            using var db = new FlowDeskDbContext();
            var tasks = db.Tasks.ToList();
            return Results.Ok(tasks.Select(t => new
            {
                t.Id,
                t.Title,
                t.Status,
                t.Priority,
                t.DueDate,
                t.ProjectId
            }));
        });
        app.MapPost("/api/tasks", async (FlowDesk.Core.Models.TaskItem task) => {
            using var db = new FlowDeskDbContext();
            db.Tasks.Add(task);
            await db.SaveChangesAsync();

            if (HubContext != null)
            {
                await HubContext.Clients.All.SendAsync("ReceiveTaskUpdate", task);
            }
            OnLocalApiTaskUpdated?.Invoke(task);

            return Results.Ok(task);
        });
        app.MapPut("/api/tasks/{id}", async (Guid id, FlowDesk.Core.Models.TaskItem task) => {
            using var db = new FlowDeskDbContext();
            var existing = await db.Tasks.FindAsync(id);
            if (existing != null)
            {
                var oldAssignee = existing.AssigneeId;
                
                existing.Title = task.Title;
                existing.Description = task.Description;
                existing.Status = task.Status;
                existing.Priority = task.Priority;
                existing.DueDate = task.DueDate;
                existing.ProjectId = task.ProjectId;
                existing.AssigneeId = task.AssigneeId;
                existing.UpdatedAt = DateTime.UtcNow;
                
                await db.SaveChangesAsync();

                if (HubContext != null)
                {
                    await HubContext.Clients.All.SendAsync("ReceiveTaskUpdate", existing);
                }
                OnLocalApiTaskUpdated?.Invoke(existing);
            }

            return Results.Ok();
        });
        app.MapDelete("/api/tasks/{id}", async (Guid id) => {
            using var db = new FlowDeskDbContext();
            var t = await db.Tasks.FindAsync(id);
            if(t!=null) { db.Tasks.Remove(t); await db.SaveChangesAsync(); }
            return Results.Ok();
        });
        
        // Task Comments
        app.MapGet("/api/tasks/{taskId}/comments", (Guid taskId) =>
        {
            using var db = new FlowDeskDbContext();
            var comments = db.TaskComments.Where(c => c.TaskId == taskId).OrderBy(c => c.CreatedAt).ToList();
            return Results.Ok(comments);
        });
        
        app.MapPost("/api/tasks/{taskId}/comments", async (Guid taskId, FlowDesk.Core.Models.TaskComment comment) => {
            using var db = new FlowDeskDbContext();
            comment.TaskId = taskId; // Ensure it matches the URL
            db.TaskComments.Add(comment);
            await db.SaveChangesAsync();
            
            // Broadcast to other clients
            if (HubContext != null)
            {
                await HubContext.Clients.All.SendAsync("ReceiveTaskComment", comment);
            }
            
            OnLocalApiTaskCommentCreated?.Invoke(comment);
            
            return Results.Ok(comment);
        });

        app.MapDelete("/api/comments/{id}", async (Guid id) => {
            using var db = new FlowDeskDbContext();
            var c = await db.TaskComments.FindAsync(id);
            if(c!=null) { db.TaskComments.Remove(c); await db.SaveChangesAsync(); }
            return Results.Ok();
        });

        app.MapGet("/api/members", () =>
        {
            using var db = new FlowDeskDbContext();
            var members = db.LocalUsers.ToList();
            return Results.Ok(members.Select(m => new
            {
                m.Id,
                m.Name,
                m.Role,
                m.WorkspaceId
            }));
        });

        app.MapGet("/api/documents", () =>
        {
            using var db = new FlowDeskDbContext();
            return Results.Ok(db.Documents.ToList());
        });
        app.MapPost("/api/documents", async (FlowDesk.Core.Models.Document doc) => {
            using var db = new FlowDeskDbContext();
            db.Documents.Add(doc);
            await db.SaveChangesAsync();
            return Results.Ok(doc);
        });
        app.MapPut("/api/documents/{id}", async (Guid id, FlowDesk.Core.Models.Document doc) => {
            using var db = new FlowDeskDbContext();
            db.Documents.Update(doc);
            await db.SaveChangesAsync();
            return Results.Ok();
        });
        app.MapDelete("/api/documents/{id}", async (Guid id) => {
            using var db = new FlowDeskDbContext();
            var d = await db.Documents.FindAsync(id);
            if(d!=null) { db.Documents.Remove(d); await db.SaveChangesAsync(); }
            return Results.Ok();
        });

        app.MapGet("/api/files", () =>
        {
            using var db = new FlowDeskDbContext();
            return Results.Ok(db.Files.ToList());
        });

        app.MapGet("/api/activity", () =>
        {
            using var db = new FlowDeskDbContext();
            return Results.Ok(db.ActivityLogs.ToList());
        });
        app.MapPost("/api/activity", async (FlowDesk.Core.Models.ActivityLog log) => {
            using var db = new FlowDeskDbContext();
            db.ActivityLogs.Add(log);
            await db.SaveChangesAsync();
            return Results.Ok();
        });

        app.MapGet("/api/tags", () =>
        {
            using var db = new FlowDeskDbContext();
            return Results.Ok(db.Tags.ToList());
        });

        app.MapGet("/api/requests", () =>
        {
            using var db = new FlowDeskDbContext();
            return Results.Ok(db.Requests.ToList());
        });
    }
}
