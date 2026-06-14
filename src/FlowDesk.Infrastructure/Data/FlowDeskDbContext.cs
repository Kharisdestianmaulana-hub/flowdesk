using FlowDesk.Core.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;

namespace FlowDesk.Infrastructure.Data;

public class FlowDeskDbContext : DbContext
{
    public DbSet<Workspace> Workspaces { get; set; } = null!;
    public DbSet<LocalUser> LocalUsers { get; set; } = null!;
    public DbSet<Project> Projects { get; set; } = null!;
    public DbSet<TaskItem> Tasks { get; set; } = null!;
    public DbSet<ActivityLog> ActivityLogs { get; set; } = null!;
    public DbSet<Document> Documents { get; set; } = null!;
    public DbSet<FileItem> Files { get; set; } = null!;
    public DbSet<RequestItem> Requests { get; set; } = null!;

    public DbSet<Tag> Tags { get; set; } = null!;
    public DbSet<ProjectTag> ProjectTags { get; set; } = null!;
    public DbSet<TaskTag> TaskTags { get; set; } = null!;

    public string DbPath { get; }

    public FlowDeskDbContext()
    {
        var customPath = Environment.GetEnvironmentVariable("FLOWDESK_DATA_DIR");
        var folder = string.IsNullOrWhiteSpace(customPath) 
            ? Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FlowDeskData")
            : customPath;
        
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }
        
        DbPath = Path.Join(folder, "flowdesk.db");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite($"Data Source={DbPath}");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<Workspace>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired();
        });
        
        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired();
        });

        modelBuilder.Entity<TaskItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired();
        });

        modelBuilder.Entity<Document>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired();
        });

        modelBuilder.Entity<FileItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.StoredPath).IsRequired();
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired();
        });

        modelBuilder.Entity<ProjectTag>(entity =>
        {
            entity.HasKey(e => new { e.ProjectId, e.TagId });
            
            entity.HasOne(e => e.Project)
                  .WithMany(p => p.ProjectTags)
                  .HasForeignKey(e => e.ProjectId);
                  
            entity.HasOne(e => e.Tag)
                  .WithMany(t => t.ProjectTags)
                  .HasForeignKey(e => e.TagId);
        });

        modelBuilder.Entity<TaskTag>(entity =>
        {
            entity.HasKey(e => new { e.TaskId, e.TagId });
            
            entity.HasOne(e => e.Task)
                  .WithMany(t => t.TaskTags)
                  .HasForeignKey(e => e.TaskId);
                  
            entity.HasOne(e => e.Tag)
                  .WithMany(t => t.TaskTags)
                  .HasForeignKey(e => e.TagId);
        });
    }
}
