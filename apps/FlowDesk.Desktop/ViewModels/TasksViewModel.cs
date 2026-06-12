using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FlowDesk.Core.Enums;
using FlowDesk.Core.Models;
using FlowDesk.Infrastructure.Services;
using System.Collections.ObjectModel;
using System.Linq;

namespace FlowDesk.Desktop.ViewModels;

public partial class TasksViewModel : ViewModelBase
{
    private readonly TaskService _taskService = new();
    private readonly ProjectService _projectService = new();

    [ObservableProperty]
    private ObservableCollection<TaskItem> _tasks = new();

    [ObservableProperty]
    private ObservableCollection<TaskItem> _filteredTasks = new();

    [ObservableProperty]
    private ObservableCollection<Project> _projects = new();

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    public bool IsEmpty => FilteredTasks.Count == 0;

    [ObservableProperty]
    private string _newTaskTitle = string.Empty;

    public TasksViewModel()
    {
        LoadTasks();
    }

    private void LoadTasks()
    {
        var dbTasks = _taskService.GetTasks();
        Tasks = new ObservableCollection<TaskItem>(dbTasks);

        var dbProjects = _projectService.GetProjects();
        Projects = new ObservableCollection<Project>(dbProjects);

        ApplyFilter();
    }

    partial void OnSearchQueryChanged(string value)
    {
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        if (string.IsNullOrWhiteSpace(SearchQuery))
        {
            FilteredTasks = new ObservableCollection<TaskItem>(Tasks);
        }
        else
        {
            var query = SearchQuery.ToLowerInvariant();
            var filtered = Tasks.Where(t => 
                t.Title.ToLowerInvariant().Contains(query) || 
                (t.Description != null && t.Description.ToLowerInvariant().Contains(query))
            );
            FilteredTasks = new ObservableCollection<TaskItem>(filtered);
        }
        OnPropertyChanged(nameof(IsEmpty));
    }

    [RelayCommand]
    private void CreateTask()
    {
        if (string.IsNullOrWhiteSpace(NewTaskTitle)) return;

        var task = _taskService.CreateTask(
            NewTaskTitle,
            null,
            FlowDesk.Core.Enums.TaskStatus.ToDo,
            TaskPriority.Medium
        );

        Tasks.Insert(0, task);
        NewTaskTitle = string.Empty;
        ApplyFilter();
        CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Default.Send(new FlowDesk.Desktop.Messages.ToastNotificationMessage("Task created."));
    }

    [RelayCommand]
    private void ToggleTaskStatus(TaskItem task)
    {
        if (task == null) return;

        var newStatus = task.Status == FlowDesk.Core.Enums.TaskStatus.Done 
            ? FlowDesk.Core.Enums.TaskStatus.ToDo 
            : FlowDesk.Core.Enums.TaskStatus.Done;

        var updatedTask = _taskService.UpdateTaskStatus(task.Id, newStatus);
        
        if (updatedTask != null)
        {
            var index = Tasks.IndexOf(task);
            if (index >= 0)
            {
                Tasks[index] = updatedTask;
                ApplyFilter();
            }
        }
    }

    [ObservableProperty]
    private TaskItem? _selectedTask;

    [ObservableProperty]
    private bool _isDetailOpen;

    [ObservableProperty] private string _editTitle = string.Empty;
    [ObservableProperty] private string? _editDescription;
    [ObservableProperty] private FlowDesk.Core.Enums.TaskStatus _editStatus;
    [ObservableProperty] private TaskPriority _editPriority;
    [ObservableProperty] private System.Guid? _editProjectId;

    [RelayCommand]
    private void OpenTaskDetail(TaskItem task)
    {
        SelectedTask = task;
        EditTitle = task.Title;
        EditDescription = task.Description;
        EditStatus = task.Status;
        EditPriority = task.Priority;
        EditProjectId = task.ProjectId;
        IsDetailOpen = true;
    }

    [RelayCommand]
    private void CloseTaskDetail()
    {
        IsDetailOpen = false;
        SelectedTask = null;
    }

    [RelayCommand]
    private void SaveTaskDetails()
    {
        if (SelectedTask == null || string.IsNullOrWhiteSpace(EditTitle)) return;

        var updatedTask = _taskService.UpdateTask(
            SelectedTask.Id,
            EditTitle,
            EditDescription,
            EditStatus,
            EditPriority,
            SelectedTask.DueDate,
            EditProjectId
        );

        if (updatedTask != null)
        {
            var index = Tasks.IndexOf(SelectedTask);
            if (index >= 0)
            {
                Tasks[index] = updatedTask;
                ApplyFilter();
                CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Default.Send(new FlowDesk.Desktop.Messages.ToastNotificationMessage("Task saved."));
            }
        }

        CloseTaskDetail();
    }

    [ObservableProperty]
    private bool _isDeleteConfirmOpen;

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
        if (SelectedTask != null)
        {
            _taskService.DeleteTask(SelectedTask.Id);
            Tasks.Remove(SelectedTask);
            ApplyFilter();
            CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Default.Send(new FlowDesk.Desktop.Messages.ToastNotificationMessage("Task deleted."));
        }
        CancelDelete();
        CloseTaskDetail();
    }
}
