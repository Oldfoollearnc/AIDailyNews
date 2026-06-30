using System.Collections.ObjectModel;
using System.Windows.Input;
using AIDailyNews.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AIDailyNews.ViewModels;

public class TopicViewModel : ObservableObject
{
    public TopicViewModel(ObservableCollection<Topic> topics)
    {
        Topics = topics;
        AddTopicCommand = new RelayCommand(AddTopic);
        DeleteTopicCommand = new RelayCommand(DeleteTopic);
    }

    public ObservableCollection<Topic> Topics { get; }

    private Topic? _selectedTopic;
    public Topic? SelectedTopic
    {
        get => _selectedTopic;
        set => SetProperty(ref _selectedTopic, value);
    }

    private string _newTopicName = string.Empty;
    public string NewTopicName
    {
        get => _newTopicName;
        set => SetProperty(ref _newTopicName, value);
    }

    public ICommand AddTopicCommand { get; private set; } = default!;
    public ICommand DeleteTopicCommand { get; private set; } = default!;

    private void AddTopic()
    {
        if (string.IsNullOrWhiteSpace(NewTopicName)) return;
        Topics.Add(new Topic { Name = NewTopicName.Trim() });
        NewTopicName = string.Empty;
    }

    private void DeleteTopic()
    {
        if (SelectedTopic == null) return;
        Topics.Remove(SelectedTopic);
        SelectedTopic = null;
    }
}
