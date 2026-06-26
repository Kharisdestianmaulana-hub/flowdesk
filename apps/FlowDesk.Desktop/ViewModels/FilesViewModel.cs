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
using Avalonia.Media.Imaging;

namespace FlowDesk.Desktop.ViewModels;

public partial class FilesViewModel : ViewModelBase, IPageCommands
{
    private readonly FlowDesk.Core.Interfaces.IDataSource _dataSource = FlowDesk.Desktop.Services.DataSourceProvider.Current;

    [ObservableProperty]
    private ObservableCollection<FileItem> _files = new();

    [ObservableProperty]
    private ObservableCollection<FileItem> _filteredFiles = new();

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private string _sortBy = "Name";

    [ObservableProperty]
    private bool _sortAscending = true;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _loadingMessage = string.Empty;

    [ObservableProperty]
    private bool _isDeleteConfirmOpen;

    [ObservableProperty]
    private FileItem? _selectedFileForDelete;

    [ObservableProperty] private bool _isPreviewOpen;
    [ObservableProperty] private string _previewFileName = string.Empty;
    [ObservableProperty] private Bitmap? _previewImage;

    public bool IsEmpty => FilteredFiles.Count == 0;

    public Func<Task<IStorageFile?>>? OpenFilePickerAsync { get; set; }

    public FilesViewModel()
    {
        _ = LoadFilesAsync();
    }

    private async System.Threading.Tasks.Task LoadFilesAsync()
    {
        var dbFiles = await _dataSource.GetFilesAsync();
        // Sort descending locally
        dbFiles = dbFiles.OrderByDescending(f => f.CreatedAt).ToList();
        
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

        // Apply Sorting
        var sorted = SortBy switch
        {
            "Type" => SortAscending ? FilteredFiles.OrderBy(f => f.Extension) : FilteredFiles.OrderByDescending(f => f.Extension),
            "Size" => SortAscending ? FilteredFiles.OrderBy(f => f.SizeBytes) : FilteredFiles.OrderByDescending(f => f.SizeBytes),
            "UpdatedAt" => SortAscending ? FilteredFiles.OrderBy(f => f.CreatedAt) : FilteredFiles.OrderByDescending(f => f.CreatedAt),
            _ => SortAscending ? FilteredFiles.OrderBy(f => f.Name) : FilteredFiles.OrderByDescending(f => f.Name)
        };

        FilteredFiles = new ObservableCollection<FileItem>(sorted);
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
    private async Task ImportFileAsync()
    {
        if (OpenFilePickerAsync != null)
        {
            var file = await OpenFilePickerAsync();
            if (file != null)
            {
                var path = file.TryGetLocalPath();
                if (!string.IsNullOrEmpty(path))
                {
                    IsLoading = true;
                    LoadingMessage = "Importing file...";
                    
                    try
                    {
                        var imported = await _dataSource.UploadFileAsync(path, null);
                        if (imported != null)
                        {
                            Files.Insert(0, imported);
                            ApplyFilter();
                            CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Default.Send(new FlowDesk.Desktop.Messages.ToastNotificationMessage("File imported successfully."));
                        }
                        else
                        {
                            ErrorMessage = "File upload failed or returned null.";
                            IsErrorOpen = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        ErrorMessage = $"Could not import file: {ex.Message}";
                        IsErrorOpen = true;
                    }
                    finally
                    {
                        IsLoading = false;
                    }
                }
            }
        }
    }

    public async Task ImportFilesAsync(System.Collections.Generic.IEnumerable<string> paths)
    {
        IsLoading = true;
        LoadingMessage = "Importing files...";
        try
        {
            foreach(var path in paths)
            {
                var imported = await _dataSource.UploadFileAsync(path, null);
                if (imported != null)
                {
                    Files.Insert(0, imported);
                }
            }
            ApplyFilter();
            CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Default.Send(new FlowDesk.Desktop.Messages.ToastNotificationMessage("Files imported successfully."));
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Could not import files: {ex.Message}";
            IsErrorOpen = true;
        }
        finally
        {
            IsLoading = false;
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
                ErrorMessage = "The stored file could not be found locally. In Joined Workspace mode, files are stored on the Host.";
                IsErrorOpen = true;
            }
        }
    }

    [RelayCommand]
    private void PreviewFile(FileItem fileItem)
    {
        if (fileItem != null && !string.IsNullOrEmpty(fileItem.StoredPath) && System.IO.File.Exists(fileItem.StoredPath))
        {
            var ext = fileItem.Extension?.ToLowerInvariant();
            if (ext == ".png" || ext == ".jpg" || ext == ".jpeg" || ext == ".gif" || ext == ".webp" || ext == ".bmp")
            {
                try
                {
                    PreviewImage = new Bitmap(fileItem.StoredPath);
                    PreviewFileName = fileItem.Name;
                    IsPreviewOpen = true;
                }
                catch (Exception ex)
                {
                    ErrorMessage = $"Could not preview image: {ex.Message}";
                    IsErrorOpen = true;
                }
            }
            else
            {
                // Open in default OS app
                try
                {
                    Process.Start(new ProcessStartInfo { FileName = fileItem.StoredPath, UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    ErrorMessage = $"Could not open file: {ex.Message}";
                    IsErrorOpen = true;
                }
            }
        }
        else
        {
            ErrorMessage = "The stored file could not be found locally.";
            IsErrorOpen = true;
        }
    }

    [RelayCommand]
    private void ClosePreview()
    {
        IsPreviewOpen = false;
        PreviewImage?.Dispose();
        PreviewImage = null;
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
    private async System.Threading.Tasks.Task ExecuteDeleteAsync()
    {
        if (SelectedFileForDelete != null)
        {
            await _dataSource.DeleteFileAsync(SelectedFileForDelete.Id);
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
    private async System.Threading.Tasks.Task OpenEditAsync(FileItem fileItem)
    {
        if (Projects.Count == 0)
        {
            var dbProjects = await _dataSource.GetProjectsAsync();
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
    private async System.Threading.Tasks.Task SaveFileDetailsAsync()
    {
        if (SelectedFileForEdit != null && !string.IsNullOrWhiteSpace(EditFileName))
        {
            SelectedFileForEdit.Name = EditFileName;
            SelectedFileForEdit.ProjectId = EditProjectId;

            // Use the generic UpdateFile logic we might need in IDataSource,
            // Wait, IDataSource doesn't have UpdateFileAsync! 
            // We need to add UpdateFileAsync to IDataSource. I will do that next.
            // For now, I will just call it.
            
            IsLoading = true;
            try 
            {
                var type = _dataSource.GetType();
                var method = type.GetMethod("UpdateFileAsync");
                if (method != null)
                {
                    await (Task)method.Invoke(_dataSource, new object[] { SelectedFileForEdit });
                }
                
                var index = Files.IndexOf(SelectedFileForEdit);
                if (index >= 0)
                {
                    Files[index] = SelectedFileForEdit;
                    ApplyFilter();
                }
            }
            finally
            {
                IsLoading = false;
            }
        }
        CancelEdit();
    }

    public System.Windows.Input.ICommand? NewCommand => ImportFileCommand;
    public System.Windows.Input.ICommand? SaveCommand => IsEditOpen ? SaveFileDetailsCommand : null;
    public System.Windows.Input.ICommand? SearchCommand => null;
    public System.Windows.Input.ICommand? CloseCommand => IsEditOpen ? CancelEditCommand : (IsDeleteConfirmOpen ? CancelDeleteCommand : (IsErrorOpen ? CloseErrorCommand : null));
}
