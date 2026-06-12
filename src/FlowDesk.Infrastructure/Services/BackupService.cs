using System;
using System.IO;
using System.Linq;
using FlowDesk.Infrastructure.Data;

namespace FlowDesk.Infrastructure.Services;

public class BackupService
{
    private readonly string _dataFolder;
    private readonly string _dbPath;
    private readonly string _autoBackupFolder;
    private readonly string _manualBackupFolder;

    public BackupService()
    {
        _dataFolder = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FlowDeskData");
        _dbPath = Path.Join(_dataFolder, "flowdesk.db");
        _autoBackupFolder = Path.Join(_dataFolder, "backups", "auto");
        _manualBackupFolder = Path.Join(_dataFolder, "backups", "manual");
    }

    public void InitializeAutoBackup()
    {
        EnsureDirectories();
        
        if (!File.Exists(_dbPath))
            return; // Nothing to backup

        var today = DateTime.Now.ToString("yyyyMMdd");
        
        // Check if we already backed up today
        var existingBackups = Directory.GetFiles(_autoBackupFolder, $"flowdesk_{today}_*.db");
        if (existingBackups.Length > 0)
        {
            return; // Already backed up today
        }

        PerformBackup(_autoBackupFolder);
        EnforceRetention(_autoBackupFolder, 7);
    }

    public string PerformManualBackup()
    {
        EnsureDirectories();
        
        if (!File.Exists(_dbPath))
            throw new FileNotFoundException("Database file not found. Nothing to backup.");

        return PerformBackup(_manualBackupFolder);
    }

    private string PerformBackup(string destinationFolder)
    {
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var backupFileName = $"flowdesk_{timestamp}.db";
        var backupFilePath = Path.Join(destinationFolder, backupFileName);

        File.Copy(_dbPath, backupFilePath, true);
        return backupFilePath;
    }

    private void EnforceRetention(string folder, int maxFiles)
    {
        var files = Directory.GetFiles(folder, "flowdesk_*.db")
                             .Select(f => new FileInfo(f))
                             .OrderByDescending(f => f.CreationTime)
                             .ToList();

        if (files.Count > maxFiles)
        {
            var filesToDelete = files.Skip(maxFiles);
            foreach (var file in filesToDelete)
            {
                try
                {
                    file.Delete();
                }
                catch
                {
                    // Ignore deletion errors for old backups
                }
            }
        }
    }

    private void EnsureDirectories()
    {
        if (!Directory.Exists(_autoBackupFolder))
            Directory.CreateDirectory(_autoBackupFolder);
            
        if (!Directory.Exists(_manualBackupFolder))
            Directory.CreateDirectory(_manualBackupFolder);
    }

    public string GetBackupFolder()
    {
        return Path.Join(_dataFolder, "backups");
    }
}
