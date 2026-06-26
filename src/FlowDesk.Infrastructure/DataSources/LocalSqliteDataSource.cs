using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FlowDesk.Core.Interfaces;
using FlowDesk.Core.Models;
using FlowDesk.Infrastructure.Data;

namespace FlowDesk.Infrastructure.DataSources;

public class LocalSqliteDataSource : IDataSource
{
    // Projects
    public async Task<List<Project>> GetProjectsAsync()
    {
        using var db = new FlowDeskDbContext();
        return await db.Projects.ToListAsync();
    }

    public async Task<Project?> GetProjectAsync(Guid id)
    {
        using var db = new FlowDeskDbContext();
        return await db.Projects.FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Project> CreateProjectAsync(Project project)
    {
        using var db = new FlowDeskDbContext();
        var workspaceId = await db.Workspaces.Select(w => w.Id).FirstOrDefaultAsync();
        var user = await db.LocalUsers.FirstOrDefaultAsync();
        project.WorkspaceId = workspaceId == Guid.Empty ? Guid.NewGuid() : workspaceId;
        
        db.Projects.Add(project);
        
        var log = new ActivityLog
        {
            Action = "Created",
            TargetType = "Project",
            TargetId = project.Id,
            ActorName = user?.Name ?? "System",
            WorkspaceId = project.WorkspaceId
        };
        db.ActivityLogs.Add(log);

        await db.SaveChangesAsync();
        return project;
    }

    public async Task UpdateProjectAsync(Project project)
    {
        using var db = new FlowDeskDbContext();
        db.Projects.Update(project);
        
        var user = await db.LocalUsers.FirstOrDefaultAsync();
        var log = new ActivityLog
        {
            Action = "Updated details",
            TargetType = "Project",
            TargetId = project.Id,
            ActorName = user?.Name ?? "System",
            WorkspaceId = project.WorkspaceId
        };
        db.ActivityLogs.Add(log);

        await db.SaveChangesAsync();
    }

    public async Task DeleteProjectAsync(Guid id)
    {
        using var db = new FlowDeskDbContext();
        var project = await db.Projects.FindAsync(id);
        if (project != null)
        {
            // Safely unlink all child data instead of cascading delete
            var tasks = await db.Tasks.Where(t => t.ProjectId == id).ToListAsync();
            foreach (var t in tasks) t.ProjectId = null;

            var docs = await db.Documents.Where(d => d.ProjectId == id).ToListAsync();
            foreach (var d in docs) d.ProjectId = null;

            var files = await db.Files.Where(f => f.ProjectId == id).ToListAsync();
            foreach (var f in files) f.ProjectId = null;

            var requests = await db.Requests.Where(r => r.ProjectId == id).ToListAsync();
            foreach (var r in requests) r.ProjectId = null;

            db.Projects.Remove(project);
            
            var user = await db.LocalUsers.FirstOrDefaultAsync();
            var log = new ActivityLog
            {
                Action = "Deleted",
                TargetType = "Project",
                TargetId = id,
                ActorName = user?.Name ?? "System",
                WorkspaceId = project.WorkspaceId
            };
            db.ActivityLogs.Add(log);

            await db.SaveChangesAsync();
        }
    }

    // Tasks
    public async Task<List<TaskItem>> GetTasksAsync()
    {
        using var db = new FlowDeskDbContext();
        return await db.Tasks.Include(t => t.TaskTags).ThenInclude(tt => tt.Tag).ToListAsync();
    }

    public async Task<TaskItem?> GetTaskAsync(Guid id)
    {
        using var db = new FlowDeskDbContext();
        return await db.Tasks.Include(t => t.TaskTags).ThenInclude(tt => tt.Tag).FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<TaskItem> CreateTaskAsync(TaskItem task)
    {
        using var db = new FlowDeskDbContext();
        if (task.TaskTags != null)
        {
            foreach (var tt in task.TaskTags)
            {
                if (tt.Tag != null) db.Tags.Attach(tt.Tag);
            }
        }
        db.Tasks.Add(task);
        await db.SaveChangesAsync();
        return task;
    }

    public async Task UpdateTaskAsync(TaskItem task)
    {
        using var db = new FlowDeskDbContext();
        db.Tasks.Update(task);
        await db.SaveChangesAsync();
    }

    public async Task DeleteTaskAsync(Guid id)
    {
        using var db = new FlowDeskDbContext();
        var task = await db.Tasks.FindAsync(id);
        if (task != null)
        {
            db.Tasks.Remove(task);
            await db.SaveChangesAsync();
        }
    }

    // Comments
    public async Task<List<TaskComment>> GetTaskCommentsAsync(Guid taskId)
    {
        using var db = new FlowDeskDbContext();
        return await db.TaskComments
            .Where(c => c.TaskId == taskId)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<TaskComment> CreateCommentAsync(TaskComment comment)
    {
        using var db = new FlowDeskDbContext();
        db.TaskComments.Add(comment);
        await db.SaveChangesAsync();
        return comment;
    }

    public async Task DeleteCommentAsync(Guid id)
    {
        using var db = new FlowDeskDbContext();
        var comment = await db.TaskComments.FindAsync(id);
        if (comment != null)
        {
            db.TaskComments.Remove(comment);
            await db.SaveChangesAsync();
        }
    }

    // Tags
    public async Task<List<Tag>> GetTagsAsync()
    {
        using var db = new FlowDeskDbContext();
        return await db.Tags.ToListAsync();
    }

    public async Task<Tag?> GetTagAsync(Guid id)
    {
        using var db = new FlowDeskDbContext();
        return await db.Tags.FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<Tag> CreateTagAsync(Tag tag)
    {
        using var db = new FlowDeskDbContext();
        db.Tags.Add(tag);
        await db.SaveChangesAsync();
        return tag;
    }

    public async Task UpdateTagAsync(Tag tag)
    {
        using var db = new FlowDeskDbContext();
        db.Tags.Update(tag);
        await db.SaveChangesAsync();
    }

    public async Task DeleteTagAsync(Guid id)
    {
        using var db = new FlowDeskDbContext();
        var tag = await db.Tags.FindAsync(id);
        if (tag != null)
        {
            db.Tags.Remove(tag);
            await db.SaveChangesAsync();
        }
    }

    private readonly string[] _colors = { "#4F46E5", "#10B981", "#F59E0B", "#EF4444", "#8B5CF6", "#EC4899", "#14B8A6" };

    public async Task UpdateTaskTagsAsync(Guid taskId, string tagsString)
    {
        using var db = new FlowDeskDbContext();
        var existingLinks = await db.TaskTags.Where(tt => tt.TaskId == taskId).ToListAsync();
        db.TaskTags.RemoveRange(existingLinks);

        if (string.IsNullOrWhiteSpace(tagsString))
        {
            await db.SaveChangesAsync();
            return;
        }

        var names = tagsString.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                              .Select(n => n.Trim().ToLowerInvariant())
                              .Distinct()
                              .ToList();
        var rnd = new Random();

        foreach (var name in names)
        {
            var tag = await db.Tags.FirstOrDefaultAsync(t => t.Name == name);
            if (tag == null)
            {
                tag = new Tag { Name = name, Color = _colors[rnd.Next(_colors.Length)] };
                db.Tags.Add(tag);
            }
            db.TaskTags.Add(new TaskTag { TaskId = taskId, TagId = tag.Id });
        }
        await db.SaveChangesAsync();
    }

    public async Task UpdateProjectTagsAsync(Guid projectId, string tagsString)
    {
        using var db = new FlowDeskDbContext();
        var existingLinks = await db.ProjectTags.Where(pt => pt.ProjectId == projectId).ToListAsync();
        db.ProjectTags.RemoveRange(existingLinks);

        if (string.IsNullOrWhiteSpace(tagsString))
        {
            await db.SaveChangesAsync();
            return;
        }

        var names = tagsString.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                              .Select(n => n.Trim().ToLowerInvariant())
                              .Distinct()
                              .ToList();
        var rnd = new Random();

        foreach (var name in names)
        {
            var tag = await db.Tags.FirstOrDefaultAsync(t => t.Name == name);
            if (tag == null)
            {
                tag = new Tag { Name = name, Color = _colors[rnd.Next(_colors.Length)] };
                db.Tags.Add(tag);
            }
            db.ProjectTags.Add(new ProjectTag { ProjectId = projectId, TagId = tag.Id });
        }
        await db.SaveChangesAsync();
    }

    // Requests
    public async Task<List<RequestItem>> GetRequestsAsync()
    {
        using var db = new FlowDeskDbContext();
        return await db.Requests.ToListAsync();
    }

    public async Task<RequestItem> CreateRequestAsync(RequestItem request)
    {
        using var db = new FlowDeskDbContext();
        db.Requests.Add(request);
        await db.SaveChangesAsync();
        return request;
    }

    public async Task UpdateRequestAsync(RequestItem request)
    {
        using var db = new FlowDeskDbContext();
        db.Requests.Update(request);
        await db.SaveChangesAsync();
    }

    public async Task DeleteRequestAsync(Guid id)
    {
        using var db = new FlowDeskDbContext();
        var req = await db.Requests.FindAsync(id);
        if (req != null)
        {
            db.Requests.Remove(req);
            await db.SaveChangesAsync();
        }
    }

    // Docs & Files
    public async Task<List<Document>> GetDocumentsAsync()
    {
        using var db = new FlowDeskDbContext();
        return await db.Documents.ToListAsync();
    }

    public async Task<Document> CreateDocumentAsync(Document doc)
    {
        using var db = new FlowDeskDbContext();
        db.Documents.Add(doc);
        await db.SaveChangesAsync();
        return doc;
    }

    public async Task UpdateDocumentAsync(Document doc)
    {
        using var db = new FlowDeskDbContext();
        db.Documents.Update(doc);
        await db.SaveChangesAsync();
    }

    public async Task DeleteDocumentAsync(Guid id)
    {
        using var db = new FlowDeskDbContext();
        var doc = await db.Documents.FindAsync(id);
        if (doc != null)
        {
            db.Documents.Remove(doc);
            await db.SaveChangesAsync();
        }
    }

    public async Task<List<FileItem>> GetFilesAsync()
    {
        using var db = new FlowDeskDbContext();
        return await db.Files.ToListAsync();
    }

    public async Task<FileItem> CreateFileAsync(FileItem file)
    {
        using var db = new FlowDeskDbContext();
        db.Files.Add(file);
        await db.SaveChangesAsync();
        return file;
    }

    public async Task<FileItem?> UploadFileAsync(string localPath, Guid? projectId)
    {
        if (!System.IO.File.Exists(localPath)) return null;

        var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var filesDir = System.IO.Path.Join(appData, "FlowDeskData", "Files");
        if (!System.IO.Directory.Exists(filesDir)) System.IO.Directory.CreateDirectory(filesDir);

        var fileInfo = new System.IO.FileInfo(localPath);
        var safeFileName = $"{Guid.NewGuid()}_{fileInfo.Name}";
        var storedPath = System.IO.Path.Join(filesDir, safeFileName);

        try
        {
            System.IO.File.Copy(localPath, storedPath, true);
        }
        catch (Exception ex)
        {
            return null;
        }

        using var db = new FlowDeskDbContext();
        var user = await db.LocalUsers.FirstOrDefaultAsync();

        var fileItem = new FileItem
        {
            Name = fileInfo.Name,
            OriginalPath = localPath,
            StoredPath = storedPath,
            SizeBytes = fileInfo.Length,
            Extension = fileInfo.Extension,
            ProjectId = projectId,
            CreatedAt = DateTime.UtcNow
        };

        db.Files.Add(fileItem);
        
        var log = new ActivityLog
        {
            Action = "Imported",
            TargetType = "File",
            TargetId = fileItem.Id,
            ActorName = user?.Name ?? "System",
            WorkspaceId = Guid.Empty
        };
        db.ActivityLogs.Add(log);

        await db.SaveChangesAsync();
        return fileItem;
    }

    public async Task<bool> DownloadFileAsync(Guid id, string destinationPath)
    {
        using var db = new FlowDeskDbContext();
        var file = await db.Files.FindAsync(id);
        if (file == null || string.IsNullOrEmpty(file.StoredPath) || !System.IO.File.Exists(file.StoredPath)) return false;

        try
        {
            System.IO.File.Copy(file.StoredPath, destinationPath, true);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task UpdateFileAsync(FileItem file)
    {
        using var db = new FlowDeskDbContext();
        db.Files.Update(file);

        var user = await db.LocalUsers.FirstOrDefaultAsync();
        var log = new ActivityLog
        {
            Action = "Updated details",
            TargetType = "File",
            TargetId = file.Id,
            ActorName = user?.Name ?? "System",
            WorkspaceId = Guid.Empty
        };
        db.ActivityLogs.Add(log);

        await db.SaveChangesAsync();
    }

    public async Task DeleteFileAsync(Guid id)
    {
        using var db = new FlowDeskDbContext();
        var file = await db.Files.FindAsync(id);
        if (file != null)
        {
            if (!string.IsNullOrEmpty(file.StoredPath) && System.IO.File.Exists(file.StoredPath))
            {
                System.IO.File.Delete(file.StoredPath);
            }
            db.Files.Remove(file);
            
            var user = await db.LocalUsers.FirstOrDefaultAsync();
            var log = new ActivityLog
            {
                Action = "Deleted",
                TargetType = "File",
                TargetId = id,
                ActorName = user?.Name ?? "System",
                WorkspaceId = Guid.Empty
            };
            db.ActivityLogs.Add(log);

            await db.SaveChangesAsync();
        }
    }

    // Activity Log
    public async Task<List<ActivityLog>> GetActivityLogsAsync()
    {
        using var db = new FlowDeskDbContext();
        return await db.ActivityLogs.ToListAsync();
    }

    public async Task LogActivityAsync(ActivityLog log)
    {
        using var db = new FlowDeskDbContext();
        db.ActivityLogs.Add(log);
        await db.SaveChangesAsync();
    }

    // Members
    public async Task<List<LocalUser>> GetMembersAsync()
    {
        using var db = new FlowDeskDbContext();
        return await db.LocalUsers.ToListAsync();
    }
}
