using FlowDesk.Core.Models;
using FlowDesk.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FlowDesk.Infrastructure.Services;

public class FileService
{
    private readonly ActivityLogService _logService = new();
    private readonly string _filesDirectory;

    public FileService()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        _filesDirectory = Path.Join(appData, "FlowDeskData", "Files");
        
        if (!Directory.Exists(_filesDirectory))
        {
            Directory.CreateDirectory(_filesDirectory);
        }
    }

    public string GetDataFolder() => _filesDirectory;

    public List<FileItem> GetFiles()
    {
        using var db = new FlowDeskDbContext();
        return db.Files.OrderByDescending(f => f.CreatedAt).ToList();
    }

    public List<FileItem> GetFilesForProject(Guid projectId)
    {
        using var db = new FlowDeskDbContext();
        return db.Files.Where(f => f.ProjectId == projectId).OrderByDescending(f => f.CreatedAt).ToList();
    }

    public FileItem? ImportFile(string originalPath, Guid? projectId)
    {
        if (!File.Exists(originalPath)) return null;

        var fileInfo = new FileInfo(originalPath);
        var safeFileName = $"{Guid.NewGuid()}_{fileInfo.Name}";
        var storedPath = Path.Join(_filesDirectory, safeFileName);

        try
        {
            File.Copy(originalPath, storedPath, true);
        }
        catch (Exception ex)
        {
            new ExceptionLogger().LogException(ex);
            return null; // Safe exit
        }

        using var db = new FlowDeskDbContext();
        var user = db.LocalUsers.FirstOrDefault();
        var actorName = user?.Name ?? "System";

        var fileItem = new FileItem
        {
            Name = fileInfo.Name,
            OriginalPath = originalPath,
            StoredPath = storedPath,
            SizeBytes = fileInfo.Length,
            Extension = fileInfo.Extension,
            ProjectId = projectId,
            CreatedAt = DateTime.UtcNow
        };

        db.Files.Add(fileItem);
        db.SaveChanges();

        _logService.LogActivity(actorName, "Imported", "File", fileItem.Id);

        return fileItem;
    }

    public void DeleteFile(Guid id)
    {
        using var db = new FlowDeskDbContext();
        var file = db.Files.Find(id);
        if (file == null) return;

        if (File.Exists(file.StoredPath))
        {
            File.Delete(file.StoredPath);
        }

        db.Files.Remove(file);
        db.SaveChanges();

        var user = db.LocalUsers.FirstOrDefault();
        var actorName = user?.Name ?? "System";
        _logService.LogActivity(actorName, "Deleted", "File", id);
    }

    public FileItem? UpdateFile(Guid id, string name, Guid? projectId)
    {
        using var db = new FlowDeskDbContext();
        var file = db.Files.Find(id);
        if (file == null) return null;

        file.Name = name;
        file.ProjectId = projectId;
        
        db.SaveChanges();

        var user = db.LocalUsers.FirstOrDefault();
        var actorName = user?.Name ?? "System";
        _logService.LogActivity(actorName, "Updated details", "File", file.Id);

        return file;
    }
}
