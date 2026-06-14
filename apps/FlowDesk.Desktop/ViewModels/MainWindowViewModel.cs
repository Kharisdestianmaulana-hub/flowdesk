using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;

namespace FlowDesk.Desktop.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private ViewModelBase _content = default!;

    public MainWindowViewModel()
    {
        var settings = new FlowDesk.Infrastructure.Services.LocalSettingsService().LoadSettings();
        AppZoomLevel = settings.AppZoomLevel > 0 ? settings.AppZoomLevel : 1.0;

        try 
        {
            using var db = new FlowDesk.Infrastructure.Data.FlowDeskDbContext();
            Microsoft.EntityFrameworkCore.RelationalDatabaseFacadeExtensions.Migrate(db.Database);
            var workspace = System.Linq.Enumerable.FirstOrDefault(db.Workspaces);
            if (workspace != null)
            {
                if (workspace.Mode == FlowDesk.Core.Enums.WorkspaceMode.Joined)
                {
                    // Joined clients do not persist across restarts for now
                    db.Database.EnsureDeleted();
                    Microsoft.EntityFrameworkCore.RelationalDatabaseFacadeExtensions.Migrate(db.Database);
                    Content = new WelcomeViewModel(this);
                }
                else
                {
                    Content = new MainViewModel(this);
                }
            }
            else
            {
                Content = new WelcomeViewModel(this);
            }
        }
        catch
        {
            // Fallback in case DB is locked or fails
            Content = new WelcomeViewModel(this);
        }
    }

    public void NavigateTo(ViewModelBase viewModel)
    {
        Content = viewModel;
    }

    [ObservableProperty]
    private double _appZoomLevel = 1.0;

    [RelayCommand]
    public void ZoomIn()
    {
        if (AppZoomLevel < 2.0)
        {
            AppZoomLevel = Math.Round(AppZoomLevel + 0.1, 1);
            SaveZoomLevel();
        }
    }

    [RelayCommand]
    public void ZoomOut()
    {
        if (AppZoomLevel > 0.5)
        {
            AppZoomLevel = Math.Round(AppZoomLevel - 0.1, 1);
            SaveZoomLevel();
        }
    }

    [RelayCommand]
    public void ResetZoom()
    {
        AppZoomLevel = 1.0;
        SaveZoomLevel();
    }

    private void SaveZoomLevel()
    {
        var settingsService = new FlowDesk.Infrastructure.Services.LocalSettingsService();
        var settings = settingsService.LoadSettings();
        settings.AppZoomLevel = AppZoomLevel;
        settingsService.SaveSettings(settings);
    }
}
