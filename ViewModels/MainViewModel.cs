using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using AIDailyNews.Models;
using AIDailyNews.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaterialDesignThemes.Wpf;

namespace AIDailyNews.ViewModels;

public class MainViewModel : ObservableObject
{
    private readonly DataService _dataService = new();
    private readonly RssService _rssService = new();
    private readonly System.Timers.Timer _refreshTimer = new(30 * 60 * 1000);
    private readonly SemaphoreSlim _refreshLock = new(1, 1);
    private List<NewsItem> _allNews = new();

    private static readonly Topic _allTopic = new() { Id = "", Name = "全部主题" };

    public NotificationService NotificationService { get; } = new();

    public MainViewModel()
    {
        Topics = _dataService.LoadTopics();
        _allNews = _dataService.LoadCachedNews();
        _rssService.SeedExistingKeys(_allNews);
        InitCommands();
        RebuildFilterItems();
        ApplyFilters();

        _refreshTimer.Elapsed += async (_, _) => await RefreshAsync();
        _refreshTimer.Start();

        _ = RefreshAsync();
    }

    private void InitCommands()
    {
        RefreshCommand = new AsyncRelayCommand(RefreshAsync);
        MarkAsReadCommand = new RelayCommand<NewsItem>(MarkAsRead);
        OpenLinkCommand = new RelayCommand<NewsItem>(OpenLink);
        ManageTopicsCommand = new AsyncRelayCommand(ManageTopicsAsync);
        ShowWindowCommand = new RelayCommand(ShowWindow);
        ExitCommand = new RelayCommand(Exit);
    }

    public ObservableCollection<Topic> Topics { get; private set; } = new();
    public ObservableCollection<Topic> TopicFilterItems { get; } = new();
    public ObservableCollection<NewsItem> FilteredNews { get; } = new();

    private void RebuildFilterItems()
    {
        TopicFilterItems.Clear();
        TopicFilterItems.Add(_allTopic);
        foreach (var t in Topics)
            TopicFilterItems.Add(t);
    }

    private Topic? _selectedTopic = _allTopic;
    public Topic? SelectedTopic
    {
        get => _selectedTopic;
        set
        {
            if (SetProperty(ref _selectedTopic, value))
            {
                _lastTopicId = value?.Id ?? string.Empty;
                ApplyFilters();
            }
        }
    }

    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
                ApplyFilters();
        }
    }

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    private string _statusText = "就绪";
    public string StatusText
    {
        get => _statusText;
        set => SetProperty(ref _statusText, value);
    }

    private string _lastTopicId = string.Empty;

    public ICommand RefreshCommand { get; private set; } = default!;
    public ICommand MarkAsReadCommand { get; private set; } = default!;
    public ICommand OpenLinkCommand { get; private set; } = default!;
    public ICommand ManageTopicsCommand { get; private set; } = default!;
    public ICommand ShowWindowCommand { get; private set; } = default!;
    public ICommand ExitCommand { get; private set; } = default!;

    private void ApplyFilters()
    {
        var filtered = _allNews.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(_lastTopicId))
            filtered = filtered.Where(n => n.TopicId == _lastTopicId);

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var kw = SearchText.ToLower();
            filtered = filtered.Where(n =>
                n.Title.ToLower().Contains(kw) ||
                n.Summary.ToLower().Contains(kw));
        }

        filtered = filtered.OrderByDescending(n => n.PublishDate);

        FilteredNews.Clear();
        foreach (var item in filtered)
            FilteredNews.Add(item);
    }

    private async Task RefreshAsync()
    {
        if (!await _refreshLock.WaitAsync(0))
        {
            StatusText = "正在刷新中，请稍候...";
            return;
        }

        try
        {
            IsLoading = true;
            StatusText = "正在获取最新资讯...";
            Topics = _dataService.LoadTopics();
            RebuildFilterItems();

            var newItems = await _rssService.FetchAllAsync(Topics);

            var failed = _rssService.FailedSources;
            var failTip = failed.Count > 0 ? $" ({failed.Count} 个源失败)" : "";

            if (newItems.Count > 0)
            {
                _allNews.InsertRange(0, newItems);
                _dataService.SaveCachedNews(_allNews);
                ApplyFilters();

                foreach (var item in newItems.Take(3))
                    NotificationService.ShowNewsNotification(item);

                StatusText = $"已获取 {newItems.Count} 条新资讯{failTip}";
            }
            else
            {
                StatusText = $"暂无新资讯{failTip}";
            }
        }
        catch (Exception ex)
        {
            StatusText = $"获取失败: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
            _refreshLock.Release();
        }
    }

    private void MarkAsRead(NewsItem? item)
    {
        if (item == null) return;
        item.IsRead = true;
        _dataService.SaveCachedNews(_allNews);
    }

    private void OpenLink(NewsItem? item)
    {
        if (item == null || string.IsNullOrWhiteSpace(item.Link)) return;
        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = item.Link,
                UseShellExecute = true
            });
        }
        catch { }
    }

    private async Task ManageTopicsAsync()
    {
        var vm = new TopicViewModel(Topics);
        var dialog = new Views.TopicDialog { DataContext = vm };
        var result = await DialogHost.Show(dialog, "RootDialog");

        if (result is bool saved && saved)
        {
            _dataService.SaveTopics(Topics);
            Topics = _dataService.LoadTopics();
            RebuildFilterItems();
        }
    }

    private void ShowWindow()
    {
        if (Application.Current.MainWindow != null)
        {
            Application.Current.MainWindow.Show();
            Application.Current.MainWindow.WindowState = WindowState.Normal;
            Application.Current.MainWindow.Activate();
        }
    }

    private void Exit()
    {
        Application.Current.Shutdown();
    }
}
