using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FlowDesk.Core.Enums;
using FlowDesk.Core.Models;
using FlowDesk.Infrastructure.Services;
using System.Collections.ObjectModel;
using System.Linq;

namespace FlowDesk.Desktop.ViewModels;

public partial class ProjectsViewModel : ViewModelBase
{
    private readonly ProjectService _projectService = new();

    [ObservableProperty]
    private ObservableCollection<Project> _projects = new();

    [ObservableProperty]
    private ObservableCollection<Project> _filteredProjects = new();

    [ObservableProperty]
    private string _searchQuery = string.Empty;

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
        LoadProjects();
    }

    private void LoadProjects()
    {
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
        OnPropertyChanged(nameof(IsEmpty));
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
    private void CreateProject()
    {
        if (string.IsNullOrWhiteSpace(NewProjectName)) return;

        var project = _projectService.CreateProject(
            NewProjectName, 
            NewProjectDescription, 
            ProjectStatus.Planning, 
            ProjectType.General
        );

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
}
