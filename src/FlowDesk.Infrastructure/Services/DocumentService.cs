using FlowDesk.Core.Models;
using FlowDesk.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FlowDesk.Infrastructure.Services;

public class DocumentService
{
    private readonly ActivityLogService _logService = new();

    public List<Document> GetDocuments()
    {
        using var db = new FlowDeskDbContext();
        return db.Documents.OrderByDescending(d => d.UpdatedAt).ToList();
    }

    public List<Document> GetDocumentsForProject(Guid projectId)
    {
        using var db = new FlowDeskDbContext();
        return db.Documents.Where(d => d.ProjectId == projectId).OrderByDescending(d => d.UpdatedAt).ToList();
    }

    public Document CreateDocument(string title, string content, Guid? projectId)
    {
        using var db = new FlowDeskDbContext();
        var user = db.LocalUsers.FirstOrDefault();
        var actorName = user?.Name ?? "System";

        var doc = new Document
        {
            Title = title,
            Content = content,
            ProjectId = projectId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.Documents.Add(doc);
        db.SaveChanges();

        _logService.LogActivity(actorName, "Created", "Document", doc.Id);

        return doc;
    }

    public Document? UpdateDocument(Guid id, string title, string content, Guid? projectId)
    {
        using var db = new FlowDeskDbContext();
        var doc = db.Documents.Find(id);
        if (doc == null) return null;

        doc.Title = title;
        doc.Content = content;
        doc.ProjectId = projectId;
        doc.UpdatedAt = DateTime.UtcNow;
        
        db.SaveChanges();

        var user = db.LocalUsers.FirstOrDefault();
        var actorName = user?.Name ?? "System";
        _logService.LogActivity(actorName, "Updated", "Document", doc.Id);

        return doc;
    }

    public void DeleteDocument(Guid id)
    {
        using var db = new FlowDeskDbContext();
        var doc = db.Documents.Find(id);
        if (doc != null)
        {
            db.Documents.Remove(doc);
            db.SaveChanges();

            var user = db.LocalUsers.FirstOrDefault();
            var actorName = user?.Name ?? "System";
            _logService.LogActivity(actorName, "Deleted", "Document", id);
        }
    }
}
