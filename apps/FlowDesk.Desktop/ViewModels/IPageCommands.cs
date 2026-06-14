using System.Windows.Input;

namespace FlowDesk.Desktop.ViewModels;

public interface IPageCommands
{
    ICommand? NewCommand { get; }
    ICommand? SaveCommand { get; }
    ICommand? SearchCommand { get; }
    ICommand? CloseCommand { get; }
}
