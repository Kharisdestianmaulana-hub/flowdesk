using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FlowDesk.Desktop.Models;
using FlowDesk.Desktop.Services;

namespace FlowDesk.Desktop.ViewModels;

public partial class OmnibarViewModel : ViewModelBase
{
    private readonly GlobalSearchService _searchService;
    private CancellationTokenSource? _searchCts;

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private bool _isOpen = false;

    [ObservableProperty]
    private bool _isLoading = false;

    [ObservableProperty]
    private ObservableCollection<SearchResultItem> _results = new();

    public Action<SearchResultItem>? OnResultSelected { get; set; }

    public OmnibarViewModel()
    {
        _searchService = new GlobalSearchService();
    }

    partial void OnSearchQueryChanged(string value)
    {
        _searchCts?.Cancel();
        _searchCts = new CancellationTokenSource();
        
        if (string.IsNullOrWhiteSpace(value))
        {
            Results.Clear();
            IsLoading = false;
            return;
        }

        IsLoading = true;
        _ = PerformSearchAsync(value, _searchCts.Token);
    }

    private async Task PerformSearchAsync(string query, CancellationToken token)
    {
        try
        {
            // Debounce delay
            await Task.Delay(300, token);

            if (token.IsCancellationRequested) return;

            var rawResults = await _searchService.SearchAsync(query, token);
            
            if (token.IsCancellationRequested) return;

            // Group by Type implicitly just by adding them (Service already ordered them, but we can group them here)
            Results = new ObservableCollection<SearchResultItem>(rawResults.OrderBy(r => r.Type));
        }
        catch (OperationCanceledException)
        {
            // Ignore cancellation
        }
        finally
        {
            if (!token.IsCancellationRequested)
            {
                IsLoading = false;
            }
        }
    }

    [RelayCommand]
    public void Open()
    {
        IsOpen = true;
        SearchQuery = string.Empty;
        Results.Clear();
    }

    [RelayCommand]
    public void Close()
    {
        IsOpen = false;
        SearchQuery = string.Empty;
    }

    [RelayCommand]
    public void SelectResult(SearchResultItem item)
    {
        if (item == null) return;
        
        Close();
        OnResultSelected?.Invoke(item);
    }
}
