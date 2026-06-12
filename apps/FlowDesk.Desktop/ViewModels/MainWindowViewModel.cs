using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace FlowDesk.Desktop.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private ViewModelBase _content = default!;

    public MainWindowViewModel()
    {
        try 
        {
            using var db = new FlowDesk.Infrastructure.Data.FlowDeskDbContext();
            // Ensure created is useful if it's the very first time. App.axaml.cs might do this too.
            db.Database.EnsureCreated();
            if (System.Linq.Enumerable.Any(db.Workspaces))
            {
                Content = new MainViewModel(this);
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
}
