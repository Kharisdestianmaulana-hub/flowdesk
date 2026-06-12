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

    public string DbPath { get; }

    public FlowDeskDbContext()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var folder = Path.Join(appData, "FlowDeskData");
        
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
    }
}
