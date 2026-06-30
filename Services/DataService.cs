using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using AIDailyNews.Models;

namespace AIDailyNews.Services;

public class DataService
{
    private readonly string _topicsPath;
    private readonly string _newsCachePath;

    public DataService()
    {
        var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AIDailyNews");
        Directory.CreateDirectory(dir);
        _topicsPath = Path.Combine(dir, "topics.json");
        _newsCachePath = Path.Combine(dir, "news_cache.json");
    }

    public ObservableCollection<Topic> LoadTopics()
    {
        if (!File.Exists(_topicsPath))
            return CreateDefaultTopics();

        var json = File.ReadAllText(_topicsPath);
        var topics = JsonSerializer.Deserialize<ObservableCollection<Topic>>(json);
        if (topics == null || topics.Count == 0)
            return CreateDefaultTopics();

        return topics;
    }

    public void SaveTopics(ObservableCollection<Topic> topics)
    {
        var json = JsonSerializer.Serialize(topics, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_topicsPath, json);
    }

    public List<NewsItem> LoadCachedNews()
    {
        if (!File.Exists(_newsCachePath)) return new();
        var json = File.ReadAllText(_newsCachePath);
        return JsonSerializer.Deserialize<List<NewsItem>>(json) ?? new();
    }

    public void SaveCachedNews(IEnumerable<NewsItem> news)
    {
        var list = news.OrderByDescending(n => n.PublishDate).Take(5000).ToList();
        var json = JsonSerializer.Serialize(list, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_newsCachePath, json);
    }

    private static ObservableCollection<Topic> CreateDefaultTopics()
    {
        return new ObservableCollection<Topic>
        {
            new() { Name = "大模型" },
            new() { Name = "多模态" },
            new() { Name = "具身智能" },
            new() { Name = "AI 芯片" },
        };
    }
}
