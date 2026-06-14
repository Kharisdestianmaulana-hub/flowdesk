using System;
using CommunityToolkit.Mvvm.ComponentModel;
using FlowDesk.Core.Models;
using FlowDesk.Infrastructure.Data;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace FlowDesk.Desktop.ViewModels;

public partial class DashboardViewModel : ViewModelBase
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasTasks))]
    private ObservableCollection<TaskItem> _tasksDueSoon = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasProjects))]
    private ObservableCollection<Project> _recentProjects = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasDocs))]
    private ObservableCollection<Document> _recentDocs = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasFiles))]
    private ObservableCollection<FileItem> _recentFiles = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasActivity))]
    private ObservableCollection<ActivityLog> _recentActivity = new();

    public bool HasTasks => TasksDueSoon.Count > 0;
    public bool HasProjects => RecentProjects.Count > 0;
    public bool HasDocs => RecentDocs.Count > 0;
    public bool HasFiles => RecentFiles.Count > 0;
    public bool HasActivity => RecentActivity.Count > 0;

    public DashboardViewModel()
    {
        _ = LoadDataAsync();
    }

    public async Task LoadDataAsync()
    {
        var dataSource = FlowDesk.Desktop.Services.DataSourceProvider.Current;
        
        var allTasks = await dataSource.GetTasksAsync();
        var allProjects = await dataSource.GetProjectsAsync();
        var allDocs = await dataSource.GetDocumentsAsync();
        var allFiles = await dataSource.GetFilesAsync();
        var allActivity = await dataSource.GetActivityLogsAsync();

        // Active Tasks (prioritize due dates)
        var dbTasks = allTasks
            .Where(t => t.Status != FlowDesk.Core.Enums.TaskStatus.Done)
            .OrderBy(t => t.DueDate.HasValue ? 0 : 1)
            .ThenBy(t => t.DueDate)
            .ThenByDescending(t => t.UpdatedAt)
            .Take(5)
            .ToList();
        TasksDueSoon = new ObservableCollection<TaskItem>(dbTasks);

        // Recent Projects
        var dbProjects = allProjects
            .OrderByDescending(p => p.UpdatedAt)
            .Take(5)
            .ToList();
        RecentProjects = new ObservableCollection<Project>(dbProjects);

        // Recent Docs
        var dbDocs = allDocs
            .OrderByDescending(d => d.UpdatedAt)
            .Take(5)
            .ToList();
        RecentDocs = new ObservableCollection<Document>(dbDocs);

        // Recent Files
        var dbFiles = allFiles
            .OrderByDescending(f => f.CreatedAt)
            .Take(5)
            .ToList();
        RecentFiles = new ObservableCollection<FileItem>(dbFiles);

        // Recent Activity
        var dbActivity = allActivity
            .OrderByDescending(a => a.CreatedAt)
            .Take(8)
            .ToList();
        RecentActivity = new ObservableCollection<ActivityLog>(dbActivity);
    }
}
