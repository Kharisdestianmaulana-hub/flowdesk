using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FlowDesk.Core.Models;
using FlowDesk.Infrastructure.Services;
using System.Collections.ObjectModel;
using System.Linq;

namespace FlowDesk.Desktop.ViewModels;

public partial class ProjectDetailViewModel : ViewModelBase
{
    private readonly MainViewModel _mainViewModel;
    private readonly TaskService _taskService = new();
    private readonly DocumentService _documentService = new();
    private readonly FileService _fileService = new();

    private readonly ProjectService _projectService = new();
    private readonly ActivityLogService _logService = new();

    [ObservableProperty]
    private Project _project;

    [ObservableProperty] private string _editName = string.Empty;
    [ObservableProperty] private string? _editDescription;
    [ObservableProperty] private FlowDesk.Core.Enums.ProjectStatus _editStatus;
    [ObservableProperty] private FlowDesk.Core.Enums.ProjectType _editType;

    [ObservableProperty]
    private ObservableCollection<TaskItem> _tasks = new();

    [ObservableProperty]
    private ObservableCollection<Document> _documents = new();

    [ObservableProperty]
    private ObservableCollection<FileItem> _files = new();

    [ObservableProperty]
    private ObservableCollection<ActivityLog> _activityLogs = new();

    public ProjectDetailViewModel(Project project, MainViewModel mainViewModel)
    {
        _project = project;
        _mainViewModel = mainViewModel;
        
        EditName = project.Name;
        EditDescription = project.Description;
        EditStatus = project.Status;
        EditType = project.Type;

        LoadData();
    }

    private void LoadData()
    {
        var dbTasks = _taskService.GetTasks().Where(t => t.ProjectId == Project.Id);
        Tasks = new ObservableCollection<TaskItem>(dbTasks);

        var dbDocs = _documentService.GetDocumentsForProject(Project.Id);
        Documents = new ObservableCollection<Document>(dbDocs);

        var dbFiles = _fileService.GetFilesForProject(Project.Id);
        Files = new ObservableCollection<FileItem>(dbFiles);

        var dbLogs = _logService.GetLogsForEntity(Project.Id);
        ActivityLogs = new ObservableCollection<ActivityLog>(dbLogs);
    }

    [RelayCommand]
    private void SaveProjectDetails()
    {
        if (string.IsNullOrWhiteSpace(EditName)) return;

        var updated = _projectService.UpdateProject(
            Project.Id,
            EditName,
            EditDescription,
            EditStatus,
            EditType
        );

        if (updated != null)
        {
            Project = updated;
            LoadData(); // reload logs
            CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Default.Send(new FlowDesk.Desktop.Messages.ToastNotificationMessage("Project details saved."));
        }
    }

    [RelayCommand]
    private void GoBack()
    {
        _mainViewModel.ChangeView(new ProjectsViewModel(_mainViewModel));
    }

    [ObservableProperty]
    private bool _isDeleteConfirmOpen;

    [RelayCommand]
    private void ArchiveProject()
    {
        var updated = _projectService.UpdateProject(
            Project.Id,
            EditName,
            EditDescription,
            FlowDesk.Core.Enums.ProjectStatus.Archived,
            EditType
        );

        if (updated != null)
        {
            Project = updated;
            EditStatus = FlowDesk.Core.Enums.ProjectStatus.Archived;
            LoadData(); // reload logs
            CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Default.Send(new FlowDesk.Desktop.Messages.ToastNotificationMessage("Project archived."));
            GoBack();
        }
    }

    [RelayCommand]
    private void ConfirmDelete()
    {
        IsDeleteConfirmOpen = true;
    }

    [RelayCommand]
    private void CancelDelete()
    {
        IsDeleteConfirmOpen = false;
    }

    [RelayCommand]
    private void ExecuteDelete()
    {
        _projectService.DeleteProject(Project.Id);
        CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Default.Send(new FlowDesk.Desktop.Messages.ToastNotificationMessage("Project deleted."));
        GoBack();
    }
}
