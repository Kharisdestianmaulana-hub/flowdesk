using FlowDesk.Core.Models;
using FlowDesk.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FlowDesk.Infrastructure.Services;

public class TagService
{
    private readonly string[] _colors = { "#4F46E5", "#10B981", "#F59E0B", "#EF4444", "#8B5CF6", "#EC4899", "#14B8A6" };

    public List<Tag> GetTagsForProject(Guid projectId)
    {
        using var db = new FlowDeskDbContext();
        return db.ProjectTags
            .Include(pt => pt.Tag)
            .Where(pt => pt.ProjectId == projectId)
            .Select(pt => pt.Tag)
            .ToList();
    }

    public List<Tag> GetTagsForTask(Guid taskId)
    {
        using var db = new FlowDeskDbContext();
        return db.TaskTags
            .Include(tt => tt.Tag)
            .Where(tt => tt.TaskId == taskId)
            .Select(tt => tt.Tag)
            .ToList();
    }

    public void UpdateProjectTags(Guid projectId, string tagsString)
    {
        using var db = new FlowDeskDbContext();
        
        // Clear existing
        var existingLinks = db.ProjectTags.Where(pt => pt.ProjectId == projectId).ToList();
        db.ProjectTags.RemoveRange(existingLinks);
        
        if (string.IsNullOrWhiteSpace(tagsString))
        {
            db.SaveChanges();
            return;
        }

        var names = tagsString.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                              .Select(n => n.Trim().ToLowerInvariant())
                              .Distinct()
                              .ToList();

        var rnd = new Random();

        foreach (var name in names)
        {
            var tag = db.Tags.FirstOrDefault(t => t.Name == name);
            if (tag == null)
            {
                tag = new Tag
                {
                    Name = name,
                    Color = _colors[rnd.Next(_colors.Length)]
                };
                db.Tags.Add(tag);
            }
            
            db.ProjectTags.Add(new ProjectTag { ProjectId = projectId, TagId = tag.Id });
        }

        db.SaveChanges();
    }

    public void UpdateTaskTags(Guid taskId, string tagsString)
    {
        using var db = new FlowDeskDbContext();
        
        var existingLinks = db.TaskTags.Where(tt => tt.TaskId == taskId).ToList();
        db.TaskTags.RemoveRange(existingLinks);
        
        if (string.IsNullOrWhiteSpace(tagsString))
        {
            db.SaveChanges();
            return;
        }

        var names = tagsString.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                              .Select(n => n.Trim().ToLowerInvariant())
                              .Distinct()
                              .ToList();

        var rnd = new Random();

        foreach (var name in names)
        {
            var tag = db.Tags.FirstOrDefault(t => t.Name == name);
            if (tag == null)
            {
                tag = new Tag
                {
                    Name = name,
                    Color = _colors[rnd.Next(_colors.Length)]
                };
                db.Tags.Add(tag);
            }
            
            db.TaskTags.Add(new TaskTag { TaskId = taskId, TagId = tag.Id });
        }

        db.SaveChanges();
    }
}
