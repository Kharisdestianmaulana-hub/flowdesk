using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FlowDesk.Core.Enums;
using FlowDesk.Core.Models;
using FlowDesk.Infrastructure.Services;
using System.Collections.ObjectModel;
using System.Linq;

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
    private string _sortBy = "Title";

    [ObservableProperty]
    private bool _sortAscending = true;

    public bool IsEmpty => FilteredTasks.Count == 0;

    [ObservableProperty] private bool _isAddOpen;
    [ObservableProperty] private string _addTitle = string.Empty;
    [ObservableProperty] private System.Guid? _addProjectId;
    [ObservableProperty] private TaskPriority _addPriority = TaskPriority.Medium;
    [ObservableProperty] private string _addDescription = string.Empty;
    [ObservableProperty] private string _addTagsString = string.Empty;
    [ObservableProperty] private System.DateTime? _addDueDate;

    public ObservableCollection<TaskPriority> Priorities { get; } = new(System.Enum.GetValues<TaskPriority>());

    public TasksViewModel()
    {
        _ = LoadTasksAsync();
    }

    private async System.Threading.Tasks.Task LoadTasksAsync()
    {
        var dbTasks = await _dataSource.GetTasksAsync();
        Tasks = new ObservableCollection<TaskItem>(dbTasks);

        var dbProjects = await _dataSource.GetProjectsAsync();
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
            Status = FlowDesk.Core.Enums.TaskStatus.ToDo,
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
    [ObservableProperty] private System.DateTime? _editDueDate;

    [RelayCommand]
    private void OpenTaskDetail(TaskItem task)
    {
        SelectedTask = task;
        EditTitle = task.Title;
        EditDescription = task.Description;
        EditStatus = task.Status;
        EditPriority = task.Priority;
        EditProjectId = task.ProjectId;
        EditDueDate = task.DueDate;
        
        EditTagsString = string.Join(", ", task.TaskTags?.Select(tt => tt.Tag?.Name) ?? System.Array.Empty<string>());
        
        IsDetailOpen = true;
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
        SelectedTask.UpdatedAt = System.DateTime.UtcNow;

        await _dataSource.UpdateTaskAsync(SelectedTask);
        await _dataSource.UpdateTaskTagsAsync(SelectedTask.Id, EditTagsString);
        
        var updatedTask = await _dataSource.GetTaskAsync(SelectedTask.Id);

        if (updatedTask != null)
        {
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
