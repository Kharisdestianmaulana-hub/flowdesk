using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FlowDesk.Core.Enums;
using FlowDesk.Core.Models;
using FlowDesk.Infrastructure.Services;
using System.Collections.ObjectModel;
using System.Linq;

namespace FlowDesk.Desktop.ViewModels;

public partial class RequestsViewModel : ViewModelBase
{
    private readonly RequestService _requestService = new();
    private readonly ProjectService _projectService = new();

    [ObservableProperty]
    private ObservableCollection<RequestItem> _requests = new();

    [ObservableProperty]
    private ObservableCollection<RequestItem> _filteredRequests = new();

    [ObservableProperty]
    private ObservableCollection<Project> _projects = new();

    public bool IsEmpty => FilteredRequests.Count == 0;

    [ObservableProperty]
    private RequestStatus? _selectedStatusFilter;

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    public RequestsViewModel()
    {
        LoadData();
    }

    private void LoadData()
    {
        var dbRequests = _requestService.GetRequests();
        Requests = new ObservableCollection<RequestItem>(dbRequests);
        
        var dbProjects = _projectService.GetProjects();
        Projects = new ObservableCollection<Project>(dbProjects);

        ApplyFilter();
    }

    partial void OnSelectedStatusFilterChanged(RequestStatus? value)
    {
        ApplyFilter();
    }

    partial void OnSearchQueryChanged(string value)
    {
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        var query = Requests.AsEnumerable();

        if (SelectedStatusFilter.HasValue)
        {
            query = query.Where(r => r.Status == SelectedStatusFilter.Value);
        }

        if (!string.IsNullOrWhiteSpace(SearchQuery))
        {
            var searchLower = SearchQuery.ToLowerInvariant();
            query = query.Where(r => 
                r.Title.ToLowerInvariant().Contains(searchLower) || 
                (r.Description != null && r.Description.ToLowerInvariant().Contains(searchLower)) ||
                (r.RequesterName != null && r.RequesterName.ToLowerInvariant().Contains(searchLower))
            );
        }

        FilteredRequests = new ObservableCollection<RequestItem>(query);
        OnPropertyChanged(nameof(IsEmpty));
    }

    [RelayCommand]
    private void ClearFilter()
    {
        SelectedStatusFilter = null;
    }

    [RelayCommand]
    private void CreateRequest()
    {
        var req = _requestService.CreateRequest(
            "New Request",
            "",
            RequestType.Feature,
            RequestPriority.Medium,
            "",
            null
        );

        Requests.Insert(0, req);
        ApplyFilter();
        OpenDetail(req);
        CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Default.Send(new FlowDesk.Desktop.Messages.ToastNotificationMessage("Request created."));
    }

    [ObservableProperty] private bool _isDetailOpen;
    [ObservableProperty] private RequestItem? _selectedRequest;

    [ObservableProperty] private string _editTitle = string.Empty;
    [ObservableProperty] private string? _editDescription;
    [ObservableProperty] private RequestType _editType;
    [ObservableProperty] private RequestPriority _editPriority;
    [ObservableProperty] private RequestStatus _editStatus;
    [ObservableProperty] private string _editRequesterName = string.Empty;
    [ObservableProperty] private System.Guid? _editProjectId;

    [RelayCommand]
    private void OpenDetail(RequestItem request)
    {
        SelectedRequest = request;
        EditTitle = request.Title;
        EditDescription = request.Description;
        EditType = request.Type;
        EditPriority = request.Priority;
        EditStatus = request.Status;
        EditRequesterName = request.RequesterName;
        EditProjectId = request.ProjectId;

        IsDetailOpen = true;
    }

    [RelayCommand]
    private void CloseDetail()
    {
        IsDetailOpen = false;
        SelectedRequest = null;
    }

    [RelayCommand]
    private void SaveDetails()
    {
        if (SelectedRequest == null || string.IsNullOrWhiteSpace(EditTitle)) return;

        var updated = _requestService.UpdateRequest(
            SelectedRequest.Id,
            EditTitle,
            EditDescription,
            EditType,
            EditPriority,
            EditStatus,
            EditRequesterName,
            EditProjectId
        );

        if (updated != null)
        {
            var index = Requests.IndexOf(SelectedRequest);
            if (index >= 0)
            {
                Requests[index] = updated;
                ApplyFilter();
                CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Default.Send(new FlowDesk.Desktop.Messages.ToastNotificationMessage("Request saved."));
            }
        }

        CloseDetail();
    }

    [ObservableProperty]
    private bool _isDeleteConfirmOpen;

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
        if (SelectedRequest != null)
        {
            _requestService.DeleteRequest(SelectedRequest.Id);
            Requests.Remove(SelectedRequest);
            ApplyFilter();
            CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Default.Send(new FlowDesk.Desktop.Messages.ToastNotificationMessage("Request deleted."));
        }
        CancelDelete();
        CloseDetail();
    }
}
