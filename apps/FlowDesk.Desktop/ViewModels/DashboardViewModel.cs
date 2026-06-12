using CommunityToolkit.Mvvm.ComponentModel;
using FlowDesk.Core.Models;
using FlowDesk.Infrastructure.Services;
using System.Collections.ObjectModel;
using System.Linq;

namespace FlowDesk.Desktop.ViewModels;

public partial class DashboardViewModel : ViewModelBase
{
    private readonly TaskService _taskService = new();
    private readonly ProjectService _projectService = new();

    [ObservableProperty]
    private ObservableCollection<TaskItem> _tasks = new();

    [ObservableProperty]
    private ObservableCollection<Project> _projects = new();

    public bool HasTasks => Tasks.Count > 0;
    public bool HasProjects => Projects.Count > 0;

    public DashboardViewModel()
    {
        var dbTasks = _taskService.GetTasks().Where(t => t.Status != FlowDesk.Core.Enums.TaskStatus.Done).Take(5);
        Tasks = new ObservableCollection<TaskItem>(dbTasks);

        var dbProjects = _projectService.GetProjects().Take(5);
        Projects = new ObservableCollection<Project>(dbProjects);
    }
}
