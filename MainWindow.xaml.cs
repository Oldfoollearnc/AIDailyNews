using System.Drawing;
using System.Windows;
using AIDailyNews.ViewModels;

namespace AIDailyNews;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        CreateTrayIcon();
        SetNotificationService();
    }

    private void SetNotificationService()
    {
        if (DataContext is MainViewModel vm)
            vm.NotificationService.SetTrayIcon(TrayIcon);
    }

    private void CreateTrayIcon()
    {
        try
        {
            using var bmp = new Bitmap(16, 16);
            using var g = Graphics.FromImage(bmp);
            g.Clear(Color.Transparent);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using var brush = new SolidBrush(Color.FromArgb(63, 81, 181));
            g.FillEllipse(brush, 0, 0, 15, 15);
            using var font = new Font("Segoe UI", 8f, System.Drawing.FontStyle.Bold);
            g.DrawString("AI", font, Brushes.White, new PointF(8, 8),
                new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });

            var hIcon = bmp.GetHicon();
            var icon = System.Drawing.Icon.FromHandle(hIcon);
            TrayIcon.Icon = icon;
        }
        catch { }
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        e.Cancel = true;
        Hide();
    }

    private void Window_StateChanged(object? sender, EventArgs e)
    {
        if (WindowState == WindowState.Minimized)
            Hide();
    }
}
