using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Markup.Xaml;
using FlowDesk.Desktop.ViewModels;
using FlowDesk.Desktop.Views;

namespace FlowDesk.Desktop;

public partial class App : Avalonia.Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Global Exception Handling
            System.AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                if (e.ExceptionObject is System.Exception ex)
                {
                    new Infrastructure.Services.ExceptionLogger().LogException(ex);
                }
            };

            System.Threading.Tasks.TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                new Infrastructure.Services.ExceptionLogger().LogException(e.Exception);
                e.SetObserved();
            };

            // Auto Backup
            new Infrastructure.Services.BackupService().InitializeAutoBackup();

            // Initialize database
            using (var db = new Infrastructure.Data.FlowDeskDbContext())
            {
                Microsoft.EntityFrameworkCore.RelationalDatabaseFacadeExtensions.Migrate(db.Database);
            }

            // Apply theme
            var settings = new Infrastructure.Services.LocalSettingsService().LoadSettings();
            Avalonia.Application.Current.RequestedThemeVariant = settings.ThemePreference switch
            {
                "Light" => Avalonia.Styling.ThemeVariant.Light,
                "Dark" => Avalonia.Styling.ThemeVariant.Dark,
                _ => Avalonia.Styling.ThemeVariant.Default
            };

            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}