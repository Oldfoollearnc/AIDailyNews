using System.Text.Json.Serialization;

namespace AIDailyNews.Models;

public class Topic
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString("N")[..8];

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    public override string ToString() => Name;
}
