using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FlowDesk.Core.Enums;
using FlowDesk.Core.Models;
using FlowDesk.Infrastructure.Services;
using System.Collections.ObjectModel;
using System.Linq;

namespace FlowDesk.Desktop.ViewModels;

public partial class ProjectsViewModel : ViewModelBase, IPageCommands
{
    private readonly FlowDesk.Core.Interfaces.IDataSource _dataSource = FlowDesk.Desktop.Services.DataSourceProvider.Current;

    [ObservableProperty]
    private ObservableCollection<Project> _projects = new();

    [ObservableProperty]
    private ObservableCollection<Project> _filteredProjects = new();

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private string _sortBy = "Name";

    [ObservableProperty]
    private bool _sortAscending = true;

    public bool IsEmpty => FilteredProjects.Count == 0;

    [ObservableProperty]
    private bool _isCreateModalOpen;

    [ObservableProperty]
    private string _newProjectName = string.Empty;

    [ObservableProperty]
    private string? _newProjectDescription;

    private readonly MainViewModel _mainViewModel;

    public ProjectsViewModel(MainViewModel mainViewModel)
    {
        _mainViewModel = mainViewModel;
        _ = LoadProjectsAsync();
    }

    private async System.Threading.Tasks.Task LoadProjectsAsync()
    {
        var dbProjects = await _dataSource.GetProjectsAsync();
        // Sort descending locally to match previous behavior
        dbProjects = dbProjects.OrderByDescending(p => p.CreatedAt).ToList();
        
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
            FilteredProjects = new ObservableCollection<Project>(Projects);
        }
        else
        {
            var query = SearchQuery.ToLowerInvariant();
            var filtered = Projects.Where(p => 
                p.Name.ToLowerInvariant().Contains(query) || 
                (p.Description != null && p.Description.ToLowerInvariant().Contains(query))
            );
            FilteredProjects = new ObservableCollection<Project>(filtered);
        }

        // Apply Sorting
        var sorted = SortBy switch
        {
            "Status" => SortAscending ? FilteredProjects.OrderBy(p => p.Status) : FilteredProjects.OrderByDescending(p => p.Status),
            "UpdatedAt" => SortAscending ? FilteredProjects.OrderBy(p => p.UpdatedAt) : FilteredProjects.OrderByDescending(p => p.UpdatedAt),
            "DueDate" => SortAscending ? FilteredProjects.OrderBy(p => p.DueDate ?? System.DateTime.MaxValue) : FilteredProjects.OrderByDescending(p => p.DueDate ?? System.DateTime.MinValue),
            _ => SortAscending ? FilteredProjects.OrderBy(p => p.Name) : FilteredProjects.OrderByDescending(p => p.Name)
        };

        FilteredProjects = new ObservableCollection<Project>(sorted);
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
    private void OpenCreateModal()
    {
        NewProjectName = string.Empty;
        NewProjectDescription = null;
        IsCreateModalOpen = true;
    }

    [RelayCommand]
    private void CloseCreateModal()
    {
        IsCreateModalOpen = false;
    }

    [RelayCommand]
    private async System.Threading.Tasks.Task CreateProjectAsync()
    {
        if (string.IsNullOrWhiteSpace(NewProjectName)) return;

        var newProject = new Project
        {
            Name = NewProjectName,
            Description = NewProjectDescription,
            Status = ProjectStatus.Planning,
            Type = ProjectType.General,
            CreatedAt = System.DateTime.UtcNow,
            UpdatedAt = System.DateTime.UtcNow
        };

        var project = await _dataSource.CreateProjectAsync(newProject);

        Projects.Insert(0, project);
        ApplyFilter();
        
        IsCreateModalOpen = false;
        CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Default.Send(new FlowDesk.Desktop.Messages.ToastNotificationMessage("Project created successfully."));
    }

    [RelayCommand]
    private void NavigateToDetail(Project project)
    {
        if (project != null)
        {
            _mainViewModel.ChangeView(new ProjectDetailViewModel(project, _mainViewModel));
        }
    }

    [ObservableProperty]
    private bool _isDeleteConfirmOpen;

    [ObservableProperty]
    private Project? _selectedProjectForDelete;

    [RelayCommand]
    private void ConfirmDelete(Project project)
    {
        SelectedProjectForDelete = project;
        IsDeleteConfirmOpen = true;
    }

    [RelayCommand]
    private void CancelDelete()
    {
        IsDeleteConfirmOpen = false;
        SelectedProjectForDelete = null;
    }

    [RelayCommand]
    private async System.Threading.Tasks.Task ExecuteDeleteAsync()
    {
        if (SelectedProjectForDelete != null)
        {
            await _dataSource.DeleteProjectAsync(SelectedProjectForDelete.Id);
            var projectToRemove = Projects.FirstOrDefault(p => p.Id == SelectedProjectForDelete.Id);
            if (projectToRemove != null)
            {
                Projects.Remove(projectToRemove);
            }
            ApplyFilter();
            CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Default.Send(new FlowDesk.Desktop.Messages.ToastNotificationMessage("Project deleted."));
        }
        CancelDelete();
    }

    public System.Windows.Input.ICommand? NewCommand => OpenCreateModalCommand;
    public System.Windows.Input.ICommand? SaveCommand => IsCreateModalOpen ? CreateProjectCommand : null;
    public System.Windows.Input.ICommand? SearchCommand => null;
    public System.Windows.Input.ICommand? CloseCommand => IsCreateModalOpen ? CloseCreateModalCommand : (IsDeleteConfirmOpen ? CancelDeleteCommand : null);
}
