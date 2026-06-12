using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FlowDesk.Core.Models;
using FlowDesk.Infrastructure.Services;
using System.Collections.ObjectModel;
using System.Linq;

namespace FlowDesk.Desktop.ViewModels;

public partial class DocsViewModel : ViewModelBase
{
    private readonly DocumentService _documentService = new();
    private readonly ProjectService _projectService = new();

    [ObservableProperty]
    private ObservableCollection<Document> _documents = new();

    [ObservableProperty]
    private ObservableCollection<Document> _filteredDocuments = new();

    [ObservableProperty]
    private string _searchQuery = string.Empty;

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
    
    private System.Timers.Timer? _debounceTimer;

    public DocsViewModel()
    {
        LoadDocuments();
        
        _debounceTimer = new System.Timers.Timer(1000); // 1 second debounce
        _debounceTimer.AutoReset = false;
        _debounceTimer.Elapsed += (s, e) => 
        {
            Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() => 
            {
                SaveDocument();
            });
        };
    }

    private void LoadDocuments()
    {
        var docs = _documentService.GetDocuments();
        Documents = new ObservableCollection<Document>(docs);

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
    }

    [RelayCommand]
    private void CreateDocument()
    {
        var newDoc = _documentService.CreateDocument("Untitled Document", "", null);
        Documents.Insert(0, newDoc);
        ApplyFilter();
        SelectedDocument = newDoc;
        CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Default.Send(new FlowDesk.Desktop.Messages.ToastNotificationMessage("Document created."));
    }

    [RelayCommand]
    private void SaveDocument()
    {
        if (SelectedDocument != null)
        {
            SaveStatus = "Saving...";
            try
            {
                // Update SelectedDocument with Edit values before saving
                SelectedDocument.Title = EditTitle;
                SelectedDocument.Content = EditContent;

                var updated = _documentService.UpdateDocument(SelectedDocument.Id, SelectedDocument.Title, SelectedDocument.Content, SelectedDocument.ProjectId);
                if (updated != null)
                {
                    var index = Documents.IndexOf(SelectedDocument);
                    if (index >= 0)
                    {
                        // Don't replace the object to avoid losing focus, just update properties
                        Documents[index].Title = updated.Title;
                        Documents[index].UpdatedAt = updated.UpdatedAt;
                        ApplyFilter();
                    }
                    SaveStatus = "Saved";
                }
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
    private void ExecuteDelete()
    {
        if (SelectedDocument != null)
        {
            _documentService.DeleteDocument(SelectedDocument.Id);
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
        OnPropertyChanged(nameof(IsDocumentSelected));
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
}
