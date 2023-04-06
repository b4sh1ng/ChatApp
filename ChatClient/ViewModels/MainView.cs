using ChatClient.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Windows;

namespace ChatClient.ViewModels;

public partial class MainView : BaseView
{
    public ObservableCollection<MessageModel>? Messages { get; set; }
    public ObservableCollection<ChatModel>? Chats { get; set; }

    [ObservableProperty]
    private bool friendsIsSelected;
    [ObservableProperty]
    private BaseView? selectedView;
    [ObservableProperty]
    private ChatModel? selectedChat;
    [ObservableProperty]
    public string? message;


    public MainView()
    {
        SelectedView = new FriendsView();
        FriendsIsSelected = true;
        Messages = new ObservableCollection<MessageModel>();
        Chats = new ObservableCollection<ChatModel>();
        

        for (int i = 0; i < 2; i++)
        {
            Messages.Add(new MessageModel
            {
                Username = "Michael",
                Time = DateTime.Now,
                Message = "Lorem ipsum dolor sit amet, consectetur adipisici elit, sed eiusmod tempor incidunt ut labore et dolore magna aliqua." +
                "Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquid ex ea commodi consequat. Quis aute " +
                "iure reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint obcaecat cupiditat non proident," +
                "sunt in culpa qui officia deserunt mollit anim id est laborum."
            });
        }
        for (int i = 0; i < 2; i++)
        {
            Messages.Add(new MessageModel
            {
                Username = "Michael",
                Time = DateTime.Now,
                Message = "Kleiner Testtext."
            });
        }

        for (int i = 0; i < 10; i++)
        {
            Chats.Add(new ChatModel
            {
                ChatName = $"Chat {i + 1}",
                Messages = this.Messages
            });
        }
    }
    partial void OnSelectedChatChanged(ChatModel? value)
    {
        FriendsIsSelected = false;
        SelectedView = new ChatView(value);
    }

    [RelayCommand]
    private void FriendsView()
    {
        SelectedChat = null;
        SelectedView = new FriendsView();
        FriendsIsSelected = true;
    }
}