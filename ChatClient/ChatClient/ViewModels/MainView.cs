using ChatClient.Enums;
using ChatClient.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using GrpcServer;
using GrpcLogin;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Configuration;

namespace ChatClient.ViewModels;

public partial class MainView : BaseView
{
    public static ObservableCollection<FriendModel>? FriendList { get; set; } = new();
    public static ObservableCollection<ChatModel>? Chats { get; set; } = new();
    public static ObservableCollection<MessageModel>? Messages { get; set; } = new();
    private static readonly GrpcChannel Channel = GrpcChannel.ForAddress("http://localhost:5292");
    public Chat.ChatClient ChatClient { get; } = new Chat.ChatClient(Channel);
    public Sign.SignClient SignClient { get; } = new Sign.SignClient(Channel);
    private TaskCompletionSource<bool>? taskCompletion;

    [ObservableProperty]
    private string? sessionId;
    [ObservableProperty]
    private int userId;
    [ObservableProperty]
    private string? username;
    [ObservableProperty]
    private string? usernameId;
    [ObservableProperty]
    private bool friendsIsSelected;
    [ObservableProperty]
    private static BaseView? selectedView;
    [ObservableProperty]
    private static ChatModel? selectedChat;
    [ObservableProperty]
    private string? message;
    [ObservableProperty]
    private string? userImageSource;
    [ObservableProperty]
    private SolidColorBrush userStatus;
    [ObservableProperty]
    private string? searchTerm;
    [ObservableProperty]
    private string? searchAnswer;
    [ObservableProperty]
    private SolidColorBrush searchAnswerColor;
    public MainView()
    {
        Application.Current.MainWindow.Loaded += Starting;
        Application.Current.MainWindow.Closing += MainWindow_Closing!;
        if (int.TryParse(ConfigurationManager.AppSettings.Get("userId"), out int id))
        {
            UserId = id;
        }
        SessionId = ConfigurationManager.AppSettings.Get("sessionId");
        FriendsIsSelected = true;
        GetUserData(ChatClient).Wait();
        Task.Run(async () =>
        {
            await GetUserChats(ChatClient);
            await GetFriendListData(ChatClient);
            SelectedView = new FriendsView(FriendList);
            await Subscribe(ChatClient, UserId);
        });
    }
    partial void OnSelectedChatChanged(ChatModel? value)
    {
        FriendsIsSelected = false;
        SelectedView = new ChatView(value, ChatClient, UserId, SessionId);
    }
    [RelayCommand]
    private void ChangeView(string? parameter)
    {
        if (parameter == "friends")
        {
            SelectedChat = null;
            SelectedView = new FriendsView(FriendList);
            FriendsIsSelected = true;
        }
    }
    [RelayCommand]
    private async void OpenChat(int friendId)
    {
        taskCompletion = new();
        var response = ChatClient.GetChatId(new ChatRequest
        {
            UserId = UserId,
            FriendId = friendId,
            SessionId = this.SessionId
        });
        var chatId = response.ChatData.ChatId;
        var isChatInList = Chats?.Where(x => x.ChatId == chatId).Select(x => x.ChatId).SingleOrDefault();
        if (isChatInList == 0)
        {
            await taskCompletion.Task;
        }
        SelectedChat = Chats?.Where(x => x.ChatId == chatId).Single();
        FriendsIsSelected = false;
        var updateChat = Chats.First(x => x.ChatId == chatId);
        updateChat.IsChatListed = true;
        SelectedView = new ChatView(Chats?
            .Where(x => x.ChatId == chatId)
            .Single(), ChatClient, UserId, SessionId);
    }
    [RelayCommand]
    private async void AcceptFriend(int friendId)
    {
        await ChatClient.FriendRequestingAsync(new FriendRequest
        {
            UserId = UserId,
            FriendId = friendId,
            IsAccepted = true,
            SessionId = this.SessionId
        });
        FriendList.SingleOrDefault(x => x.FriendId == friendId).IsFriend = true;

    }
    [RelayCommand]
    private async void DenyFriend(int friendId)
    {
        await ChatClient.FriendRequestingAsync(new FriendRequest
        {
            UserId = UserId,
            FriendId = friendId,
            IsAccepted = false,
            SessionId = this.SessionId
        });
        var toRemove = FriendList?.FirstOrDefault(f => f.FriendId == friendId);
        if (toRemove != null)
        {
            FriendList?.Remove(toRemove);
        }
    }
    [RelayCommand]
    private async void RemoveFriend(int friendId)
    {
        var deleteRequest = await ChatClient.DeleteFriendAsync(new FriendRequest
        {
            SessionId = this.SessionId,
            UserId = this.UserId,
            FriendId = friendId,
        });
        var toRemove = FriendList?.FirstOrDefault(f => f.FriendId == friendId);
        if (deleteRequest.IsOk && toRemove != null)
        {
            FriendList?.Remove(toRemove);
        }
    }
    [RelayCommand]
    private void SetStatus(string parameter)
    {
        var status = parameter switch
        {
            "online" => 1,
            "busy" => 2,
            "invisible" => 3,
            _ => 1,
        };
        ChatClient.PostNewStatus(new NewUserStatus
        {
            UserStatus = status,
            UserId = this.UserId,
            SessionId = this.SessionId,
        });
        UserStatus = StatusEnumHandler.GetStatusColor((State)status);
    }
    [RelayCommand]
    private void Logout()
    {
        SignClient.Logout(new SessionLogin
        {
            SessionId = SessionId,
            UserId = this.UserId
        });
        Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        config.AppSettings.Settings["userId"].Value = "";
        config.AppSettings.Settings["sessionId"].Value = "";
        config.Save(ConfigurationSaveMode.Full, true);
        Application.Current.Shutdown();
    }
    [RelayCommand]
    private void TryFriendRequest(string parameter)
    {
        var request = ChatClient.PostFriendRequest(new FriendRequestSearch
        {
            SearchTerm = parameter,
            UserId = this.UserId,
            SessionId = this.SessionId,
        });
        if (request.IsOk == false)
        {
            SearchAnswer = $"Folgenden Nutzer \"{parameter}\" nicht gefunden oder ist schon ein Freund. :o";
            SearchAnswerColor = new SolidColorBrush(Colors.IndianRed);
        }
        else
        {
            SearchAnswer = $"Folgenden Nutzer \"{parameter}\" Anfrage geschickt! :)";
            SearchAnswerColor = new SolidColorBrush(Colors.Green);
        }
    }
    private async Task Subscribe(Chat.ChatClient client, int chatId)
    {
        var sub = client.Subscribe(new Request
        {
            Id = UserId,
            SessionId = this.SessionId
        });

        try
        {
            await foreach (var response in sub.ResponseStream.ReadAllAsync())
            {
                ProcessResponseMessage(response);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
        await client.UnsubscribeAsync(new Request { Id = chatId });
    }
    private Task Unsubscribe(Chat.ChatClient client, int userId)
    {
        client.UnsubscribeAsync(new Request
        {
            Id = userId,
            SessionId = this.SessionId
        });
        return Task.CompletedTask;
    }
    private Task GetUserData(Chat.ChatClient client)
    {
        try
        {
            var response = client.GetUserData(new Request
            {
                SessionId = this.SessionId,
                Id = UserId,
            });
            Username = response.MyUsername;
            UserImageSource = response.MyProfileImgB64;
            UsernameId = $"#{response.MyUsernameId}";
            UserStatus = StatusEnumHandler.GetStatusColor((State)response.MyUserStatus);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
        return Task.CompletedTask;
    }
    private async Task GetUserChats(Chat.ChatClient client)
    {
        var response = client.GetUserChats(new Request
        {
            Id = UserId,
            SessionId = this.SessionId
        });
        try
        {
            await foreach (var chat in response.ResponseStream.ReadAllAsync())
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Chats.Add(new ChatModel
                    {
                        ChatName = chat.ChatName,
                        ChatId = chat.ChatId,
                        ImageSource = chat.ChatImgB64,
                        IsChatListed = chat.IsListed,
                        CurrentStatus = StatusEnumHandler.GetStatusColor((State)chat.CurrentStatus),
                    });
                });
                await GetChatDataFromChatId(client, chat.ChatId);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
    }
    private async Task GetChatDataFromChatId(Chat.ChatClient client, int chatId)
    {
        var request = client.GetChatData(new ChatDataRequest
        {
            ChatId = chatId,
            UserId = this.UserId,
            SessionId = this.SessionId
        });
        try
        {
            await foreach (var message in request.ResponseStream.ReadAllAsync())
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Chats?.Single(x => x.ChatId == chatId).Messages?.Add(new MessageModel
                    {
                        Username = message.Username,
                        ImageSource = message.ImageSource,
                        FromId = message.FromId,
                        Message = message.Text,
                        Time = DateTimeOffset.FromUnixTimeSeconds(message.Time).DateTime,
                        IsEdited = message.IsEdited,
                        IsRead = message.IsRead,
                    });
                });
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
    }
    private async Task GetFriendListData(Chat.ChatClient client)
    {
        // var friendList = new ObservableCollection<FriendModel>();
        var response = client.GetUserFriends(new Request
        {
            Id = UserId,
            SessionId = this.SessionId
        });
        try
        {
            await foreach (var friend in response.ResponseStream.ReadAllAsync())
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    FriendList.Add(new FriendModel
                    {
                        FriendId = friend.FriendId,
                        ImageSource = friend.FriendImgB64,
                        Username = friend.FriendUsername,
                        UsernameId = friend.FriendUserId,
                        IsFriend = friend.IsFriend,
                        CurrentStatus = StatusEnumHandler.GetStatusColor((State)friend.CurrentStatus),
                    });
                });
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"{ex.Message}");
        }
        //return friendList;
    }
    private void MainWindow_Closing(object? sender, CancelEventArgs e)
    {
        Unsubscribe(ChatClient, UserId).Wait();
    }
    private void Starting(object? sender, EventArgs e)
    {
    }
}