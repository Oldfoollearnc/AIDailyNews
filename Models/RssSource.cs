using System.Text.Json.Serialization;

namespace AIDailyNews.Models;

public class RssSource
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    public override string ToString() => Name;
}
