using ChatClient.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Grpc.Core;
using Grpc.Net.Client;
using GrpcServer;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;


namespace ChatClient.ViewModels;

public partial class MainView : BaseView
{
    public ObservableCollection<MessageModel>? Messages { get; set; } = new ObservableCollection<MessageModel>();
    public static ObservableCollection<ChatModel>? Chats { get; set; } = new ObservableCollection<ChatModel>();
    private static readonly GrpcChannel Channel = GrpcChannel.ForAddress("http://localhost:5292");
    public Chat.ChatClient client { get; } = new Chat.ChatClient(Channel);
    public ObservableCollection<FriendModel>? FriendList;

    [ObservableProperty]
    private int userId;
    [ObservableProperty]
    private string? username;
    [ObservableProperty]
    private string? usernameId;
    [ObservableProperty]
    private bool friendsIsSelected;
    [ObservableProperty]
    private BaseView? selectedView;
    [ObservableProperty]
    private ChatModel? selectedChat;
    [ObservableProperty]
    public string? message;
    [ObservableProperty]
    public string? userImageSource;
    public MainView()
    {
        Application.Current.MainWindow.Closing += MainWindow_Closing!;
        SelectedView = new FriendsView();
        FriendsIsSelected = true;
        GetUserData(client).Wait();

        Task.Run(async () =>
        {
            Chats = await GetUserChats(client);
            await Subscribe(client, UserId);
        });
    }
    private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        Unsubscribe(client, UserId);
    }
    partial void OnSelectedChatChanged(ChatModel? value)
    {
        FriendsIsSelected = false;
        SelectedView = new ChatView(ref value, client, UserId);
    }

    [RelayCommand]
    private void ChangeView(string? parameter)
    {
        if (parameter == "friends")
        {
            SelectedChat = null;
            SelectedView = new FriendsView();
            FriendsIsSelected = true;
        }
        else if (parameter == "settings")
        {
            SelectedChat = null;
            SelectedView = new SettingsView();
            FriendsIsSelected = false;
        }
    }
    public Task GetUserData(Chat.ChatClient client)
    {
        var response = client.GetUserData(new Login
        {
            LoginMail = "Bash",
            Password = "test",
        });
        UserId = response.MyUserid;
        Username = response.MyUsername;
        UserImageSource = response.MyProfileImgB64;
        UsernameId = $"#{response.MyUsernameId}";
        return Task.CompletedTask;
    }
    public async Task<ObservableCollection<ChatModel>> GetUserChats(Chat.ChatClient client)
    {
        var chatList = new ObservableCollection<ChatModel>();
        var response = client.GetUserChats(new Request { Id = UserId });
        await foreach (var chat in response.ResponseStream.ReadAllAsync())
        {
            chatList.Add(new ChatModel
            {
                ChatName = chat.ChatName,
                ChatId = chat.ChatId,
                Messages = await GetChatDataFromChatId(client, chat.ChatId),
                ImageSource = chat.ChatImgB64,
                IsChatListed = chat.IsListed,
            });
        }
        return chatList;
    }
    private async Task<ObservableCollection<MessageModel>> GetChatDataFromChatId(Chat.ChatClient client, int chatId)
    {
        var messageList = new ObservableCollection<MessageModel>();
        var request = client.GetChatData(new Request { Id = chatId });

        await foreach (var message in request.ResponseStream.ReadAllAsync())
        {
            messageList.Add(new MessageModel
            {
                Username = message.Username,
                ImageSource = message.ImageSource,
                FromId = message.FromId,
                Message = message.Text,
                Time = DateTimeOffset.FromUnixTimeSeconds(message.Time).DateTime,
                IsEdited = message.IsEdited,
                IsRead = message.IsRead,
            });
        }
        var subMessageList = new ObservableCollection<MessageModel>(messageList.OrderBy(x => x.Time));
        return subMessageList ;
    }
    private async Task Subscribe(Chat.ChatClient client, int chatId)
    {
        var sub = client.Subscribe(new Request { Id = UserId });

        try
        {
            await foreach (var x in sub.ResponseStream.ReadAllAsync())
            {
                ProcessResponseMessage(sub.ResponseStream.Current);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
        await client.UnsubscribeAsync(new Request { Id = chatId });
    }
    private async Task Unsubscribe(Chat.ChatClient client, int chatId)
    {
        await client.UnsubscribeAsync(new Request { Id = chatId });
    }
}