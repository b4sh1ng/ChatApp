using ChatClient.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Grpc.Core;
using Grpc.Net.Client;
using GrpcServer;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;


namespace ChatClient.ViewModels;

public partial class MainView : BaseView
{
    public ObservableCollection<MessageModel>? Messages { get; set; } = new ObservableCollection<MessageModel>();
    public ObservableCollection<ChatModel>? Chats { get; set; } = new ObservableCollection<ChatModel>();
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
    public string? userImageSource = "iVBORw0KGgoAAAANSUhEUgAAAMMAAAC9CAIAAACBNV5MAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAPcSURBVHhe7dtBdqNYDAXQ7H8HPe1d9Q769Coayj+J/ys78EFgkrr3vCFIstDUbwAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAD/B3/8+DqwVp/Mw8JU4l8VAihMZCjRxGRsCeRN3+euf/yLxQBcOEVueck0x5HvigCLx8GcoE5t9mEuJ2ZZu6D7x4hwKxE4XcwUx0sgZ3RKvz2GX2ObKvFYMM35Gt0SROWwUexzKC8UkLunFYonviXVPiQc+8xIxw9YzuiVKzWFMrO9XYsuReLjlfP0AMeSGRMHWhbVifes+Sbwy52TR/YhLmsJasbiR7xEvzjlT3zpm25wo23qxrF9crHUx8XqreY6+dQy2OVG29WJBbG3/JU05Td83BtucKNt6saDfWux0ZaJIq3y0aFp3SVOicuvIV/qVxUJXJoq0ykfrm8ZIOxPFW0eein3t+B5Rp9U/VN8x5tmZKN468lS/r9jmUKJUq3+ovmPMszNRvHXkqX5fsc2hRKlW/1B9x5hnZ6J468hT/b5im0OJUq3+ofqOMc/ORPHWkaf6fcU2hxKlWv1D9R1jnp2J4q0jT/X7im0OJUq1+ofqO8Y8OxPFW0ee6vcV2xxKlGr1D9V3jHl2Joq3jjwV+9rxPaJOq3+o6Fh6TFG5deQr/cpioSsTRVrlE/R9Y6rNibKtFwv6rcVOVyaKtMon6PvGVJsTZVsvFsTWxr9HvD7nNH3fGGxzomzrxbJ+cbHWxcTrreY5onXFMUXBOawVixv5HvHinJP13WO8DYmCrQtrxfrWfZJ4peVk0X3fMUWpOYyJ9f1KbDkSD7e8RMyw45iiTqvPmFjie2LXU+KBz7xKjLH1kqLIHDaKPQ7ltWKY8WOK1+ewS2xzZa4gRho5pnixhb1ioYu5iJjqPXE0kXj4M5SJzT7M1cR4d4kDmhIPdOEQseUpVxajbgg0cRlDgRQnshh4Km7li8AqcTcfAQAAAAAAAAAAAAAAAAAAKOFfzBT4OCPHxC4uiRouiRouiRouiRouiRouiRouiRouiRouiRouiRouiRouiRouiRouiRouiRouiRouiRouiRouiRouiTIuiRr3l+SY2M4lUcMlUcMlUcMlUcMlUSMuaQps5JKo4ZK2sbR0v5Fb+F2s6Pcws5QQC1kTZn/4UuLnbwuzWMqUHyx+aUn4FKuZ8jPEjyoMj8WabvleYvjaMCB2d8tlxZy1Ya9Y6EdeK4YpD/VixZGjRbuDwnli9c+yTRQ5OrxYfI/vEq4oPtLVwvcTn/D88KPE1z0o/HHiAtYHAAAAAAAAAAAAAAAAAAAAAICreXv7H+M2Dc0qcX9UAAAAAElFTkSuQmCC";


    public MainView()
    {        
        SelectedView = new FriendsView();
        FriendsIsSelected = true;
       
        //Messages = new ObservableCollection<MessageModel>();
        //Chats = new ObservableCollection<ChatModel>();
        GetUserData(client).Wait();
        //var sub = client.Subscribe(new Request { Id = UserId });
        Task.Run(async () => (            
            Chats = await GetUserChats(client)
            )       
        );
        //using (sub)
        //{
        //    var responseReaderTask = Task.Run(async () =>
        //    {
        //        while (await sub.ResponseStream.MoveNext())
        //        {
        //            ProcessResponseMessage(sub.ResponseStream.Current);
        //        }
        //    });
        //    await responseReaderTask;
        //}
    }
    partial void OnSelectedChatChanged(ChatModel? value)
    {
        FriendsIsSelected = false;
        SelectedView = new ChatView(ref value, client);
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
        return messageList;
    }
}