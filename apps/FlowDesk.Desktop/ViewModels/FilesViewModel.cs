using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FlowDesk.Core.Models;
using System.Linq;
using FlowDesk.Infrastructure.Services;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;

namespace FlowDesk.Desktop.ViewModels;

public partial class FilesViewModel : ViewModelBase
{
    private readonly FileService _fileService = new();

    [ObservableProperty]
    private ObservableCollection<FileItem> _files = new();

    [ObservableProperty]
    private ObservableCollection<FileItem> _filteredFiles = new();

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private bool _isDeleteConfirmOpen;

    [ObservableProperty]
    private FileItem? _selectedFileForDelete;

    public bool IsEmpty => FilteredFiles.Count == 0;

    public Func<Task<IStorageFile?>>? OpenFilePickerAsync { get; set; }

    public FilesViewModel()
    {
        LoadFiles();
    }

    private void LoadFiles()
    {
        var dbFiles = _fileService.GetFiles();
        Files = new ObservableCollection<FileItem>(dbFiles);
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
            FilteredFiles = new ObservableCollection<FileItem>(Files);
        }
        else
        {
            var query = SearchQuery.ToLowerInvariant();
            var filtered = Files.Where(f => 
                f.Name.ToLowerInvariant().Contains(query) || 
                (f.Extension != null && f.Extension.ToLowerInvariant().Contains(query))
            );
            FilteredFiles = new ObservableCollection<FileItem>(filtered);
        }
        OnPropertyChanged(nameof(IsEmpty));
    }

    [RelayCommand]
    private async Task ImportFile()
    {
        if (OpenFilePickerAsync != null)
        {
            var file = await OpenFilePickerAsync();
            if (file != null)
            {
                var path = file.TryGetLocalPath();
                if (!string.IsNullOrEmpty(path))
                {
                    var imported = _fileService.ImportFile(path, null);
                    if (imported != null)
                    {
                        Files.Insert(0, imported);
                        ApplyFilter();
                        CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Default.Send(new FlowDesk.Desktop.Messages.ToastNotificationMessage("File imported successfully."));
                    }
                }
            }
        }
    }

    [ObservableProperty] private bool _isErrorOpen;
    [ObservableProperty] private string _errorMessage = string.Empty;

    [RelayCommand]
    private void RevealInFolder(FileItem fileItem)
    {
        if (fileItem != null && !string.IsNullOrEmpty(fileItem.StoredPath))
        {
            if (System.IO.File.Exists(fileItem.StoredPath))
            {
                // MacOS reveal
                Process.Start(new ProcessStartInfo("open", $"-R \"{fileItem.StoredPath}\"") { UseShellExecute = true });
            }
            else
            {
                ErrorMessage = "The stored file could not be found. It may have been deleted outside of FlowDesk.";
                IsErrorOpen = true;
            }
        }
    }

    [RelayCommand]
    private void CloseError()
    {
        IsErrorOpen = false;
        ErrorMessage = string.Empty;
    }

    [RelayCommand]
    private void ConfirmDelete(FileItem fileItem)
    {
        SelectedFileForDelete = fileItem;
        IsDeleteConfirmOpen = true;
    }

    [RelayCommand]
    private void CancelDelete()
    {
        IsDeleteConfirmOpen = false;
        SelectedFileForDelete = null;
    }

    [RelayCommand]
    private void ExecuteDelete()
    {
        if (SelectedFileForDelete != null)
        {
            _fileService.DeleteFile(SelectedFileForDelete.Id);
            Files.Remove(SelectedFileForDelete);
            ApplyFilter();
            CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Default.Send(new FlowDesk.Desktop.Messages.ToastNotificationMessage("File deleted."));
        }
        CancelDelete();
    }

    [ObservableProperty] private bool _isEditOpen;
    [ObservableProperty] private FileItem? _selectedFileForEdit;
    [ObservableProperty] private string _editFileName = string.Empty;
    [ObservableProperty] private Guid? _editProjectId;

    [ObservableProperty]
    private ObservableCollection<Project> _projects = new();

    [RelayCommand]
    private void OpenEdit(FileItem fileItem)
    {
        if (Projects.Count == 0)
        {
            var dbProjects = new ProjectService().GetProjects();
            Projects = new ObservableCollection<Project>(dbProjects);
        }

        SelectedFileForEdit = fileItem;
        EditFileName = fileItem.Name;
        EditProjectId = fileItem.ProjectId;
        IsEditOpen = true;
    }

    [RelayCommand]
    private void CancelEdit()
    {
        IsEditOpen = false;
        SelectedFileForEdit = null;
    }

    [RelayCommand]
    private void SaveFileDetails()
    {
        if (SelectedFileForEdit != null && !string.IsNullOrWhiteSpace(EditFileName))
        {
            var updated = _fileService.UpdateFile(SelectedFileForEdit.Id, EditFileName, EditProjectId);
            if (updated != null)
            {
                var index = Files.IndexOf(SelectedFileForEdit);
                if (index >= 0)
                {
                    Files[index] = updated;
                    ApplyFilter();
                }
            }
        }
        CancelEdit();
    }
}
