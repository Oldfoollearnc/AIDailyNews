using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace AIDailyNews.Models;

public class NewsItem : INotifyPropertyChanged
{
    public string Title { get; set; } = string.Empty;
    public string Link { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public DateTime PublishDate { get; set; } = DateTime.Now;
    public string Source { get; set; } = string.Empty;
    public string TopicId { get; set; } = string.Empty;
    public string TopicName { get; set; } = string.Empty;

    private bool _isRead;
    public bool IsRead
    {
        get => _isRead;
        set
        {
            if (_isRead == value) return;
            _isRead = value;
            OnPropertyChanged();
        }
    }

    [JsonIgnore]
    public string UniqueKey => $"{Source}|{Link}";

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
