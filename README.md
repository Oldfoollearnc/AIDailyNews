# AI Daily News

A WPF desktop application that aggregates AI-related news from multiple RSS sources, filters them by user-defined topics, and presents them in a Material Design-styled interface.

## Features

- **RSS Feed Aggregation** - Pulls from 9 sources covering ArXiv, TechCrunch, MIT Tech Review, and Chinese AI media
- **Topic-based Filtering** - User-defined topics with keyword matching on title and summary
- **Keyword Search** - Real-time text search filtering
- **Mark as Read/Unread** - Visual differentiation with local persistence
- **System Tray** - Minimizes to tray with right-click menu
- **Desktop Notifications** - Balloon-tip notifications for new articles
- **Auto-refresh** - Timer-based refresh every 30 minutes
- **Topic Management** - Add/delete topics via Material Design dialog

## Tech Stack

| Layer | Technology |
|---|---|
| Framework | .NET 8.0 (WPF) |
| MVVM | CommunityToolkit.Mvvm |
| UI | MaterialDesignThemes |
| System Tray | Hardcodet.NotifyIcon.Wpf |
| RSS Parsing | System.ServiceModel.Syndication |

## Build & Run

**Prerequisites:** Windows with .NET 8.0 SDK

```bash
dotnet build
dotnet run
```

Or publish as self-contained:

```bash
dotnet publish -c Release -r win-x64 --self-contained
```

## Project Structure

```
AIDailyNews/
├── App.xaml / App.xaml.cs       # Application entry
├── MainWindow.xaml / .cs        # Main UI
├── Models/
│   ├── NewsItem.cs              # News article model
│   ├── RssSource.cs             # RSS source definition
│   └── Topic.cs                 # User-defined topic
├── Services/
│   ├── DataService.cs           # JSON persistence
│   ├── NotificationService.cs   # System tray notifications
│   └── RssService.cs            # RSS feed fetching & filtering
├── ViewModels/
│   ├── MainViewModel.cs         # Core logic
│   └── TopicViewModel.cs        # Topic management
├── Views/
│   └── TopicDialog.xaml         # Topic management dialog
└── Converters/
    └── BoolToFontWeightConverter.cs
```

## License

MIT
