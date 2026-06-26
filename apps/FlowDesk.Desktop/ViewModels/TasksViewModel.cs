using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FlowDesk.Core.Enums;
using FlowDesk.Core.Models;
using FlowDesk.Infrastructure.Services;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.AspNetCore.SignalR;

namespace FlowDesk.Desktop.ViewModels;

public partial class TasksViewModel : ViewModelBase, IPageCommands
{
    private readonly FlowDesk.Core.Interfaces.IDataSource _dataSource = FlowDesk.Desktop.Services.DataSourceProvider.Current;

    [ObservableProperty]
    private ObservableCollection<TaskItem> _tasks = new();

    [ObservableProperty]
    private ObservableCollection<TaskItem> _filteredTasks = new();

    [ObservableProperty]
    private ObservableCollection<Project> _projects = new();

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private FlowDesk.Core.Enums.TaskStatus? _filterStatus;

    [ObservableProperty]
    private TaskPriority? _filterPriority;

    [ObservableProperty]
    private string _sortBy = "Title";

    [ObservableProperty]
    private bool _sortAscending = true;

    public bool IsEmpty => FilteredTasks.Count == 0;

    [ObservableProperty] private bool _isAddOpen;
    [ObservableProperty] private string _addTitle = string.Empty;
    [ObservableProperty] private System.Guid? _addProjectId;
    [ObservableProperty] private FlowDesk.Core.Enums.TaskStatus _addStatus = FlowDesk.Core.Enums.TaskStatus.ToDo;
    [ObservableProperty] private TaskPriority _addPriority = TaskPriority.Medium;
    [ObservableProperty] private string _addDescription = string.Empty;
    [ObservableProperty] private string _addTagsString = string.Empty;
    [ObservableProperty] private System.DateTime? _addDueDate;

    public ObservableCollection<TaskPriority> Priorities { get; } = new(System.Enum.GetValues<TaskPriority>());
    public ObservableCollection<FlowDesk.Core.Enums.TaskStatus> Statuses { get; } = new(System.Enum.GetValues<FlowDesk.Core.Enums.TaskStatus>());

    [ObservableProperty]
    private ObservableCollection<LocalUser> _workspaceMembers = new();

    public TasksViewModel()
    {
        _ = LoadTasksAsync();

        CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Default.Register<FlowDesk.Desktop.Messages.TaskCommentReceivedMessage>(this, (r, m) =>
        {
            Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (SelectedTask != null && m.Comment.TaskId == SelectedTask.Id)
                {
                    CurrentTaskComments.Add(m.Comment);
                }
            });
        });

        CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Default.Register<FlowDesk.Desktop.Messages.TaskUpdatedMessage>(this, (r, m) =>
        {
            Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
            {
                var existingTask = Tasks.FirstOrDefault(t => t.Id == m.Task.Id);
                if (existingTask != null)
                {
                    var index = Tasks.IndexOf(existingTask);
                    Tasks[index] = m.Task;
                }
                else
                {
                    Tasks.Add(m.Task);
                }
                ApplyFilter();
                
                if (SelectedTask != null && SelectedTask.Id == m.Task.Id)
                {
                    SelectedTask = m.Task; // Force refresh selected task if needed
                }
            });
        });
    }

    private async System.Threading.Tasks.Task LoadTasksAsync()
    {
        var dbTasks = await _dataSource.GetTasksAsync();
        Tasks = new ObservableCollection<TaskItem>(dbTasks);

        var dbProjects = await _dataSource.GetProjectsAsync();
        Projects = new ObservableCollection<Project>(dbProjects);

        var members = await _dataSource.GetMembersAsync();
        WorkspaceMembers = new ObservableCollection<LocalUser>(members);

        ApplyFilter();
    }

    partial void OnSearchQueryChanged(string value) => ApplyFilter();
    partial void OnFilterStatusChanged(FlowDesk.Core.Enums.TaskStatus? value) => ApplyFilter();
    partial void OnFilterPriorityChanged(TaskPriority? value) => ApplyFilter();

    private void ApplyFilter()
    {
        var filtered = Tasks.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(SearchQuery))
        {
            var query = SearchQuery.ToLowerInvariant();
            filtered = filtered.Where(t => 
                t.Title.ToLowerInvariant().Contains(query) || 
                (t.Description != null && t.Description.ToLowerInvariant().Contains(query))
            );
        }

        if (FilterStatus.HasValue)
        {
            filtered = filtered.Where(t => t.Status == FilterStatus.Value);
        }

        if (FilterPriority.HasValue)
        {
            filtered = filtered.Where(t => t.Priority == FilterPriority.Value);
        }

        FilteredTasks = new ObservableCollection<TaskItem>(filtered);

        // Apply Sorting
        var sorted = SortBy switch
        {
            "Status" => SortAscending ? FilteredTasks.OrderBy(t => t.Status) : FilteredTasks.OrderByDescending(t => t.Status),
            "Priority" => SortAscending ? FilteredTasks.OrderBy(t => t.Priority) : FilteredTasks.OrderByDescending(t => t.Priority),
            "DueDate" => SortAscending ? FilteredTasks.OrderBy(t => t.DueDate ?? System.DateTime.MaxValue) : FilteredTasks.OrderByDescending(t => t.DueDate ?? System.DateTime.MinValue),
            "UpdatedAt" => SortAscending ? FilteredTasks.OrderBy(t => t.UpdatedAt) : FilteredTasks.OrderByDescending(t => t.UpdatedAt),
            _ => SortAscending ? FilteredTasks.OrderBy(t => t.Title) : FilteredTasks.OrderByDescending(t => t.Title)
        };

        FilteredTasks = new ObservableCollection<TaskItem>(sorted);
        OnPropertyChanged(nameof(IsEmpty));
    }

    [RelayCommand]
    private void Sort(string columnName)
    {
        if (SortBy == columnName)
        {
            SortAscending = !SortAscending;
        }
        else
        {
            SortBy = columnName;
            SortAscending = true;
        }
        ApplyFilter();
    }

    [RelayCommand]
    private void OpenAddModal()
    {
        AddTitle = string.Empty;
        AddProjectId = null;
        AddStatus = FlowDesk.Core.Enums.TaskStatus.ToDo;
        AddPriority = TaskPriority.Medium;
        AddDescription = string.Empty;
        AddTagsString = string.Empty;
        AddDueDate = null;
        IsAddOpen = true;
    }

    [RelayCommand]
    private void CloseAddModal()
    {
        IsAddOpen = false;
    }

    [RelayCommand]
    private async System.Threading.Tasks.Task CreateTaskAsync()
    {
        if (string.IsNullOrWhiteSpace(AddTitle)) return;

        var newTask = new TaskItem
        {
            Title = AddTitle,
            ProjectId = AddProjectId,
            Status = AddStatus,
            Priority = AddPriority,
            DueDate = AddDueDate,
            Description = AddDescription,
            CreatedAt = System.DateTime.UtcNow,
            UpdatedAt = System.DateTime.UtcNow
        };

        var task = await _dataSource.CreateTaskAsync(newTask);

        if (!string.IsNullOrWhiteSpace(AddTagsString))
        {
            await _dataSource.UpdateTaskTagsAsync(task.Id, AddTagsString);
        }
        
        // Reload task to get tags locally
        var updatedTask = await _dataSource.GetTaskAsync(task.Id);
        if (updatedTask != null) task = updatedTask;

        var workspaceService = new FlowDesk.Infrastructure.Services.WorkspaceService();
        var workspace = workspaceService.GetCurrentWorkspace();
        if (workspace != null && workspace.Mode != FlowDesk.Core.Enums.WorkspaceMode.Joined)
        {
            var hub = FlowDesk.Infrastructure.Hosting.LocalServerHost.HubContext;
            if (hub != null)
            {
                await hub.Clients.All.SendAsync("ReceiveTaskUpdate", task);
            }
        }

        Tasks.Insert(0, task);
        IsAddOpen = false;
        ApplyFilter();
        CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Default.Send(new FlowDesk.Desktop.Messages.ToastNotificationMessage("Task created."));
    }

    [RelayCommand]
    private async System.Threading.Tasks.Task ToggleTaskStatusAsync(TaskItem task)
    {
        if (task == null) return;

        var newStatus = task.Status == FlowDesk.Core.Enums.TaskStatus.Done 
            ? FlowDesk.Core.Enums.TaskStatus.ToDo 
            : FlowDesk.Core.Enums.TaskStatus.Done;

        task.Status = newStatus;
        task.UpdatedAt = System.DateTime.UtcNow;
        
        await _dataSource.UpdateTaskAsync(task);

        var workspaceService = new FlowDesk.Infrastructure.Services.WorkspaceService();
        var workspace = workspaceService.GetCurrentWorkspace();
        if (workspace != null && workspace.Mode != FlowDesk.Core.Enums.WorkspaceMode.Joined)
        {
            var hub = FlowDesk.Infrastructure.Hosting.LocalServerHost.HubContext;
            if (hub != null)
            {
                await hub.Clients.All.SendAsync("ReceiveTaskUpdate", task);
            }
        }
        
        var index = Tasks.IndexOf(task);
        if (index >= 0)
        {
            Tasks[index] = task;
            ApplyFilter();
        }
    }

    [ObservableProperty]
    private TaskItem? _selectedTask;

    [ObservableProperty]
    private bool _isDetailOpen;

    [ObservableProperty] private string _editTitle = string.Empty;
    [ObservableProperty] private string? _editDescription;
    [ObservableProperty] private string _editTagsString = string.Empty;
    [ObservableProperty] private FlowDesk.Core.Enums.TaskStatus _editStatus;
    [ObservableProperty] private TaskPriority _editPriority;
    [ObservableProperty] private System.Guid? _editProjectId;
    [ObservableProperty] private System.Guid? _editAssigneeId;
    [ObservableProperty] private LocalUser? _selectedAssignee;
    [ObservableProperty] private System.DateTime? _editDueDate;
    
    [ObservableProperty]
    private ObservableCollection<TaskComment> _currentTaskComments = new();

    [ObservableProperty]
    private string _newCommentText = string.Empty;

    [RelayCommand]
    private async System.Threading.Tasks.Task AddCommentAsync()
    {
        if (string.IsNullOrWhiteSpace(NewCommentText) || SelectedTask == null) return;
        
        var workspaceService = new FlowDesk.Infrastructure.Services.WorkspaceService();
        var user = workspaceService.GetCurrentUser();
        var authorName = user?.Name ?? "Unknown";
        var authorInitial = authorName.Length > 0 ? authorName.Substring(0, 1).ToUpper() : "?";

        var comment = new TaskComment
        {
            TaskId = SelectedTask.Id,
            AuthorName = authorName,
            AuthorInitial = authorInitial,
            Content = NewCommentText
        };

        await _dataSource.CreateCommentAsync(comment);
        
        // Let the Messenger handle the UI update if it was an API post, 
        // but here we are creating locally, so it updates local DB.
        // Wait, if we use API, we should POST to API if we are a client!
        // To abstract this, we can just call _dataSource.CreateCommentAsync, 
        // and if it's RemoteHttpDataSource, it POSTs. If it's Local, it saves to DB.
        // But Local won't broadcast to other clients if we don't do it manually here,
        // because LocalDataSource doesn't know about HubContext!
        // The proper way is to POST to the API even if local, or handle Hub here.
        var workspace = workspaceService.GetCurrentWorkspace();
        if (workspace != null && workspace.Mode != FlowDesk.Core.Enums.WorkspaceMode.Joined)
        {
            // Host mode: Broadcast directly
            var hub = FlowDesk.Infrastructure.Hosting.LocalServerHost.HubContext;
            if (hub != null)
            {
                await hub.Clients.All.SendAsync("ReceiveTaskComment", comment);
            }
            CurrentTaskComments.Add(comment);
        }
        else
        {
            // Joined mode: RemoteHttpDataSource already returns the created comment, but we don't broadcast, server does.
            CurrentTaskComments.Add(comment);
        }

        NewCommentText = string.Empty;
    }

    [RelayCommand]
    private async System.Threading.Tasks.Task OpenTaskDetail(TaskItem task)
    {
        SelectedTask = task;
        EditTitle = task.Title;
        EditDescription = task.Description;
        EditStatus = task.Status;
        EditPriority = task.Priority;
        EditProjectId = task.ProjectId;
        EditAssigneeId = task.AssigneeId;
        SelectedAssignee = WorkspaceMembers.FirstOrDefault(m => m.Id == task.AssigneeId);
        EditDueDate = task.DueDate;
        
        EditTagsString = string.Join(", ", task.TaskTags?.Select(tt => tt.Tag?.Name) ?? System.Array.Empty<string>());
        
        var comments = await _dataSource.GetTaskCommentsAsync(task.Id);
        CurrentTaskComments = new ObservableCollection<TaskComment>(comments);
        NewCommentText = string.Empty;

        IsDetailOpen = true;
    }

    partial void OnSelectedAssigneeChanged(LocalUser? value)
    {
        EditAssigneeId = value?.Id;
    }

    [RelayCommand]
    private void CloseTaskDetail()
    {
        IsDetailOpen = false;
        SelectedTask = null;
    }

    [RelayCommand]
    private async System.Threading.Tasks.Task SaveTaskDetailsAsync()
    {
        if (SelectedTask == null) return;
        
        SelectedTask.Title = EditTitle;
        SelectedTask.Description = EditDescription;
        SelectedTask.Status = EditStatus;
        SelectedTask.Priority = EditPriority;
        SelectedTask.DueDate = EditDueDate;
        SelectedTask.ProjectId = EditProjectId;
        SelectedTask.AssigneeId = EditAssigneeId;
        SelectedTask.UpdatedAt = System.DateTime.UtcNow;

        await _dataSource.UpdateTaskAsync(SelectedTask);
        await _dataSource.UpdateTaskTagsAsync(SelectedTask.Id, EditTagsString);
        
        var updatedTask = await _dataSource.GetTaskAsync(SelectedTask.Id);

        if (updatedTask != null)
        {
            var workspaceService = new FlowDesk.Infrastructure.Services.WorkspaceService();
            var workspace = workspaceService.GetCurrentWorkspace();
            if (workspace != null && workspace.Mode != FlowDesk.Core.Enums.WorkspaceMode.Joined)
            {
                var hub = FlowDesk.Infrastructure.Hosting.LocalServerHost.HubContext;
                if (hub != null)
                {
                    await hub.Clients.All.SendAsync("ReceiveTaskUpdate", updatedTask);
                }
            }

            var index = Tasks.IndexOf(SelectedTask);
            if (index >= 0)
            {
                Tasks[index] = updatedTask;
                SelectedTask = updatedTask;
                ApplyFilter();
            }
        }

        IsDetailOpen = false;
        CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Default.Send(new FlowDesk.Desktop.Messages.ToastNotificationMessage("Task updated."));
    }

    public System.Windows.Input.ICommand? NewCommand => OpenAddModalCommand;
    public System.Windows.Input.ICommand? SaveCommand => IsAddOpen ? CreateTaskCommand : (IsDetailOpen ? SaveTaskDetailsCommand : null);
    public System.Windows.Input.ICommand? SearchCommand => null;
    public System.Windows.Input.ICommand? CloseCommand => IsAddOpen ? CloseAddModalCommand : (IsDeleteConfirmOpen ? CancelDeleteCommand : (IsDetailOpen ? CloseTaskDetailCommand : null));

    [ObservableProperty]
    private bool _isDeleteConfirmOpen;

    [RelayCommand]
    private void ConfirmDelete(TaskItem? task = null)
    {
        if (task != null)
        {
            SelectedTask = task;
        }
        IsDeleteConfirmOpen = true;
    }

    [RelayCommand]
    private void CancelDelete()
    {
        IsDeleteConfirmOpen = false;
    }

    [RelayCommand]
    private async System.Threading.Tasks.Task ExecuteDeleteAsync()
    {
        if (SelectedTask != null)
        {
            await _dataSource.DeleteTaskAsync(SelectedTask.Id);
            Tasks.Remove(SelectedTask);
            ApplyFilter();
            CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Default.Send(new FlowDesk.Desktop.Messages.ToastNotificationMessage("Task deleted."));
        }
        CancelDelete();
        CloseTaskDetail();
    }
}
