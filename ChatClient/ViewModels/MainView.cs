using ChatClient.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;


namespace ChatClient.ViewModels;

public partial class MainView : BaseView
{
    public ObservableCollection<MessageModel>? Messages { get; set; }
    public ObservableCollection<ChatModel>? Chats { get; set; }

    [ObservableProperty]
    private BaseView? selectedView;
    [ObservableProperty]
    private ChatModel? selectedChat;
    [ObservableProperty]
    public string? message;
    public MainView()
    {
        SelectedView = new FriendsView();
        Chats = new ObservableCollection<ChatModel>();
        for (int i = 0; i < 10; i++)
        {
            Chats.Add(new ChatModel
            {
                ChatName = $"Chat {i + 1}",
                //Messages = Messages
            });
        }
    }
    partial void OnSelectedChatChanged(ChatModel? value)
    { 
        SelectedView = new ChatView(value);
    }
}
