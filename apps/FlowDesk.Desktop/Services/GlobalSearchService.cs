using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FlowDesk.Desktop.Models;
using FlowDesk.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FlowDesk.Desktop.Services;

public class GlobalSearchService
{
    public async Task<List<SearchResultItem>> SearchAsync(string query, CancellationToken cancellationToken = default)
    {
        var results = new List<SearchResultItem>();
        if (string.IsNullOrWhiteSpace(query))
            return results;

        var q = query.ToLowerInvariant();

        var dataSource = FlowDesk.Desktop.Services.DataSourceProvider.Current;
        
        // Fetch all data for searching (in a real app, IDataSource should expose a SearchAsync method for efficiency)
        var allProjects = await dataSource.GetProjectsAsync();
        var allTasks = await dataSource.GetTasksAsync();
        var allDocs = await dataSource.GetDocumentsAsync();
        var allFiles = await dataSource.GetFilesAsync();

        // 1. Search Projects
        var projects = allProjects
            .Where(p => p.Name.ToLowerInvariant().Contains(q) || (p.Description != null && p.Description.ToLowerInvariant().Contains(q)))
            .Take(5)
            .ToList();

        results.AddRange(projects.Select(p => new SearchResultItem
        {
            Title = p.Name,
            Subtitle = string.IsNullOrWhiteSpace(p.Description) ? "Project" : p.Description,
            Type = "Project",
            TargetId = p.Id.ToString()
        }));

        // 2. Search Tasks
        var tasks = allTasks
            .Where(t => t.Title.ToLowerInvariant().Contains(q) || (t.Description != null && t.Description.ToLowerInvariant().Contains(q)))
            .Take(5)
            .ToList();

        results.AddRange(tasks.Select(t => new SearchResultItem
        {
            Title = t.Title,
            Subtitle = $"Project ID: {t.ProjectId} • Status: {t.Status}",
            Type = "Task",
            TargetId = t.Id.ToString()
        }));

        // 3. Search Docs
        var docs = allDocs
            .Where(d => d.Title.ToLowerInvariant().Contains(q) || (d.Content != null && d.Content.ToLowerInvariant().Contains(q)))
            .Take(5)
            .ToList();

        results.AddRange(docs.Select(d => new SearchResultItem
        {
            Title = d.Title,
            Subtitle = $"Last updated: {d.UpdatedAt:MMM dd, yyyy}",
            Type = "Doc",
            TargetId = d.Id.ToString()
        }));

        // 4. Search Files
        var files = allFiles
            .Where(f => f.Name.ToLowerInvariant().Contains(q))
            .Take(5)
            .ToList();

        results.AddRange(files.Select(f => new SearchResultItem
        {
            Title = f.Name,
            Subtitle = $"Size: {f.SizeBytes / 1024} KB",
            Type = "File",
            TargetId = f.Id.ToString()
        }));

        return results;
    }
}
