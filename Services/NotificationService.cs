using AIDailyNews.Models;
using Hardcodet.Wpf.TaskbarNotification;

namespace AIDailyNews.Services;

public class NotificationService
{
    private TaskbarIcon? _trayIcon;

    public void SetTrayIcon(TaskbarIcon icon)
    {
        _trayIcon = icon;
    }

    public void ShowNewsNotification(NewsItem item)
    {
        if (_trayIcon == null) return;
        try
        {
            _trayIcon.ShowBalloonTip(
                item.TopicName,
                item.Title,
                BalloonIcon.Info);
        }
        catch { }
    }
}
