using System.Collections.ObjectModel;
using System.Net.Http;
using System.ServiceModel.Syndication;
using System.Text.RegularExpressions;
using System.Xml;
using AIDailyNews.Models;

namespace AIDailyNews.Services;

public class RssService
{
    private static readonly List<RssSource> _globalSources = new()
    {
        new() { Name = "ArXiv AI", Url = "https://rss.arxiv.org/rss/cs.AI" },
        new() { Name = "ArXiv ML", Url = "https://rss.arxiv.org/rss/cs.LG" },
        new() { Name = "ArXiv CL", Url = "https://rss.arxiv.org/rss/cs.CL" },
        new() { Name = "ArXiv CV", Url = "https://rss.arxiv.org/rss/cs.CV" },
        new() { Name = "ArXiv RO", Url = "https://rss.arxiv.org/rss/cs.RO" },
        new() { Name = "TechCrunch AI", Url = "https://techcrunch.com/category/artificial-intelligence/feed/" },
        new() { Name = "MIT Tech Review AI", Url = "https://www.technologyreview.com/topic/artificial-intelligence/feed/" },
        new() { Name = "机器之心", Url = "https://www.jiqizhixin.com/rss" },
        new() { Name = "量子位", Url = "https://www.qbitai.com/feed" },
    };

    private static readonly HttpClient _httpClient = new();
    private readonly HashSet<string> _existingKeys = new();
    private readonly List<string> _failedSources = new();

    public IReadOnlyList<string> FailedSources => _failedSources;

    public void SeedExistingKeys(IEnumerable<NewsItem> items)
    {
        _existingKeys.Clear();
        foreach (var item in items)
            _existingKeys.Add(item.UniqueKey);
    }

    public async Task<List<NewsItem>> FetchAllAsync(ObservableCollection<Topic> topics)
    {
        _failedSources.Clear();

        if (topics.Count == 0) return new();

        var topicKeywords = topics
            .Select(t => (
                Topic: t,
                Keywords: t.Name.Split(' ', '\u3000', ',', '，', ';', '；')
                    .Select(k => k.Trim().ToLower())
                    .Where(k => k.Length > 0)
                    .ToList()
            ))
            .Where(t => t.Keywords.Count > 0)
            .ToList();

        if (topicKeywords.Count == 0) return new();

        var allRawItems = new List<(string title, string link, string summary, DateTime date, string source)>();

        foreach (var source in _globalSources)
        {
            try
            {
                using var stream = await _httpClient.GetStreamAsync(source.Url);
                using var reader = XmlReader.Create(stream);
                var feed = SyndicationFeed.Load(reader);
                if (feed?.Items == null) continue;

                foreach (var item in feed.Items)
                {
                    allRawItems.Add((
                        item.Title?.Text?.Trim() ?? "(无标题)",
                        item.Links?.FirstOrDefault()?.Uri?.ToString() ?? "",
                        item.Summary?.Text?.StripHtml()?.Trim() ?? "",
                        item.PublishDate != DateTimeOffset.MinValue ? item.PublishDate.LocalDateTime : DateTime.Now,
                        source.Name
                    ));
                }
            }
            catch
            {
                _failedSources.Add(source.Name);
            }
        }

        var results = new List<NewsItem>();
        foreach (var (title, link, summary, date, sourceName) in allRawItems)
        {
            var key = $"{sourceName}|{link}";
            if (!_existingKeys.Add(key))
                continue;

            var matched = MatchTopic(title, summary, topicKeywords);
            if (matched == null)
                continue;

            results.Add(new NewsItem
            {
                Title = title,
                Link = link,
                Summary = summary,
                PublishDate = date,
                Source = sourceName,
                TopicId = matched.Id,
                TopicName = matched.Name,
            });
        }

        return results;
    }

    private static Topic? MatchTopic(string title, string summary,
        List<(Topic Topic, List<string> Keywords)> topicKeywords)
    {
        var text = (title + " " + summary).ToLower();

        foreach (var (topic, keywords) in topicKeywords)
        {
            foreach (var kw in keywords)
            {
                if (text.Contains(kw))
                    return topic;
            }
        }

        return null;
    }
}

public static class StringExtensions
{
    public static string StripHtml(this string html)
    {
        if (string.IsNullOrWhiteSpace(html)) return string.Empty;
        return Regex.Replace(html, "<.*?>", string.Empty);
    }
}
