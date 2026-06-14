using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FlowDesk.Core.Models;
using FlowDesk.Infrastructure.Services;
using System.Collections.ObjectModel;
using System.Linq;

namespace FlowDesk.Desktop.ViewModels;

public partial class DocsViewModel : ViewModelBase, IPageCommands
{
    private readonly FlowDesk.Core.Interfaces.IDataSource _dataSource = FlowDesk.Desktop.Services.DataSourceProvider.Current;

    [ObservableProperty]
    private ObservableCollection<Document> _documents = new();

    [ObservableProperty]
    private ObservableCollection<Document> _filteredDocuments = new();

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private string _sortBy = "UpdatedAt";

    [ObservableProperty]
    private bool _sortAscending = false;

    [ObservableProperty]
    private ObservableCollection<Project> _projects = new();

    [ObservableProperty]
    private Document? _selectedDocument;

    [ObservableProperty]
    private bool _isDeleteConfirmOpen;

    public bool IsDocumentSelected => SelectedDocument != null;

    [ObservableProperty] private string _editTitle = string.Empty;
    [ObservableProperty] private string _editContent = string.Empty;
    [ObservableProperty] private string _saveStatus = string.Empty;
    [ObservableProperty] private bool _isPreviewMode;

    [RelayCommand]
    private void TogglePreviewMode()
    {
        IsPreviewMode = !IsPreviewMode;
    }
    
    private System.Timers.Timer? _debounceTimer;

    public DocsViewModel()
    {
        _ = LoadDocumentsAsync();
        
        _debounceTimer = new System.Timers.Timer(1000); // 1 second debounce
        _debounceTimer.AutoReset = false;
        _debounceTimer.Elapsed += (s, e) => 
        {
            Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() => 
            {
                _ = SaveDocumentAsync();
            });
        };
    }

    private async System.Threading.Tasks.Task LoadDocumentsAsync()
    {
        var docs = await _dataSource.GetDocumentsAsync();
        Documents = new ObservableCollection<Document>(docs);

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
            FilteredDocuments = new ObservableCollection<Document>(Documents);
        }
        else
        {
            var query = SearchQuery.ToLowerInvariant();
            var filtered = Documents.Where(d => 
                d.Title.ToLowerInvariant().Contains(query) || 
                d.Content.ToLowerInvariant().Contains(query)
            );
            FilteredDocuments = new ObservableCollection<Document>(filtered);
        }

        // Apply Sorting
        var sorted = SortBy switch
        {
            "Title" => SortAscending ? FilteredDocuments.OrderBy(d => d.Title) : FilteredDocuments.OrderByDescending(d => d.Title),
            "UpdatedAt" => SortAscending ? FilteredDocuments.OrderBy(d => d.UpdatedAt) : FilteredDocuments.OrderByDescending(d => d.UpdatedAt),
            _ => SortAscending ? FilteredDocuments.OrderBy(d => d.UpdatedAt) : FilteredDocuments.OrderByDescending(d => d.UpdatedAt)
        };

        FilteredDocuments = new ObservableCollection<Document>(sorted);
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
    private async System.Threading.Tasks.Task CreateDocumentAsync()
    {
        var newDoc = new Document
        {
            Title = "Untitled Document",
            Content = "",
            CreatedAt = System.DateTime.UtcNow,
            UpdatedAt = System.DateTime.UtcNow
        };
        newDoc = await _dataSource.CreateDocumentAsync(newDoc);
        Documents.Insert(0, newDoc);
        ApplyFilter();
        SelectedDocument = newDoc;
        CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Default.Send(new FlowDesk.Desktop.Messages.ToastNotificationMessage("Document created."));
    }

    [RelayCommand]
    private async System.Threading.Tasks.Task SaveDocumentAsync()
    {
        if (SelectedDocument != null)
        {
            SaveStatus = "Saving...";
            try
            {
                // Update SelectedDocument with Edit values before saving
                SelectedDocument.Title = EditTitle;
                SelectedDocument.Content = EditContent;
                SelectedDocument.UpdatedAt = System.DateTime.UtcNow;

                await _dataSource.UpdateDocumentAsync(SelectedDocument);
                
                var index = Documents.IndexOf(SelectedDocument);
                if (index >= 0)
                {
                    // Trigger property change internally implicitly
                    ApplyFilter();
                }
                SaveStatus = "Saved";
            }
            catch
            {
                SaveStatus = "Could not save";
            }
        }
    }

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
    private async System.Threading.Tasks.Task ExecuteDeleteAsync()
    {
        if (SelectedDocument != null)
        {
            await _dataSource.DeleteDocumentAsync(SelectedDocument.Id);
            Documents.Remove(SelectedDocument);
            ApplyFilter();
            SelectedDocument = null;
            CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Default.Send(new FlowDesk.Desktop.Messages.ToastNotificationMessage("Document deleted."));
        }
        CancelDelete();
    }

    partial void OnSelectedDocumentChanged(Document? value)
    {
        if (value != null)
        {
            _editTitle = value.Title;
            _editContent = value.Content;
            OnPropertyChanged(nameof(EditTitle));
            OnPropertyChanged(nameof(EditContent));
            SaveStatus = string.Empty;
        }
        else
        {
            EditTitle = string.Empty;
            EditContent = string.Empty;
            SaveStatus = string.Empty;
            _debounceTimer?.Stop();
        }
        
        OnPropertyChanged(nameof(IsDocumentSelected));
        OnPropertyChanged(nameof(SaveCommand));
        OnPropertyChanged(nameof(CloseCommand));
    }

    partial void OnEditTitleChanged(string value)
    {
        TriggerAutoSave();
    }

    partial void OnEditContentChanged(string value)
    {
        TriggerAutoSave();
    }



    private void TriggerAutoSave()
    {
        if (SelectedDocument == null) return;
        SaveStatus = "Typing...";
        _debounceTimer?.Stop();
        _debounceTimer?.Start();
    }

    public System.Windows.Input.ICommand? NewCommand => CreateDocumentCommand;
    public System.Windows.Input.ICommand? SaveCommand => IsDocumentSelected ? SaveDocumentCommand : null;
    public System.Windows.Input.ICommand? SearchCommand => null;
    public System.Windows.Input.ICommand? CloseCommand => IsDeleteConfirmOpen ? CancelDeleteCommand : null;
}
