﻿using ChatClient.Enums;
using ChatClient.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Grpc.Core;
using Grpc.Net.Client;
using GrpcLogin;
using GrpcServer;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace ChatClient.ViewModels;

public partial class MainView : BaseView
{
    public static ObservableCollection<FriendModel>? FriendList { get; set; } = new();
    public static ObservableCollection<ChatModel>? Chats { get; set; } = new();
    public static ICollectionView? ChatsCollectionView { get; set; }

    private CancellationTokenSource tokenSource;
    private readonly static SocketsHttpHandler handler = new()
    {
        PooledConnectionIdleTimeout = Timeout.InfiniteTimeSpan,
        KeepAlivePingDelay = TimeSpan.FromSeconds(15),
        KeepAlivePingTimeout = TimeSpan.FromSeconds(10),
        EnableMultipleHttp2Connections = true,
    };
    private static readonly GrpcChannel Channel = GrpcChannel.ForAddress(ConfigurationManager.AppSettings.Get("connectionString"), new GrpcChannelOptions
    {
        HttpHandler = handler
    });
    public Chat.ChatClient ChatClient { get; } = new Chat.ChatClient(Channel);
    public Sign.SignClient SignClient { get; } = new Sign.SignClient(Channel);
    private TaskCompletionSource<bool>? taskCompletion;
    #region Observable Propertys
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
    private string? userImageSource;
    [ObservableProperty]
    private SolidColorBrush userStatus;
    [ObservableProperty]
    private string? searchTerm;
    [ObservableProperty]
    private string? searchAnswer;
    [ObservableProperty]
    private SolidColorBrush searchAnswerColor;
    #endregion
    partial void OnSelectedChatChanged(ChatModel? value)
    {
        FriendsIsSelected = false;
        SelectedView = new ChatView(value, ChatClient, UserId, SessionId);
    }
    public MainView()
    {
        Application.Current.Windows
            .OfType<MainWindow>()
            .Single().Closing += MainWindow_Closing;
        SharedData.FriendListCollection = CollectionViewSource.GetDefaultView(FriendList);
        ChatsCollectionView = CollectionViewSource.GetDefaultView(Chats);
        ChatsCollectionView.SortDescriptions.Add(new SortDescription(nameof(ChatModel.LatestMessageTime), ListSortDirection.Descending));
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
            SelectedView = new FriendsView();
            await Subscribe(ChatClient, UserId);
        });
    }
    #region Commands
    [RelayCommand]
    private void ChangeView(string? parameter)
    {
        if (parameter == "friends")
        {
            SelectedChat = null;
            SelectedView = new FriendsView();
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
            SessionId = this.SessionId,
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
        var toRemove = FriendList?.Single(f => f.FriendId == friendId);
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
    private async void TryFriendRequest(string parameter)
    {
        var request = await ChatClient.PostFriendRequestAsync(new FriendRequestSearch
        {
            SearchTerm = parameter,
            UserId = this.UserId,
            SessionId = this.SessionId,
        });
        if (request.IsOk == false)
        {
            SearchAnswer = $"Following User \"{parameter}\" not found or is already a friend. :o";
            SearchAnswerColor = new SolidColorBrush(Colors.IndianRed);
        }
        else
        {
            SearchAnswer = $"Sent to following User \"{parameter}\" a request! :)";
            SearchAnswerColor = new SolidColorBrush(Colors.Green);
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
    private async void Logout()
    {
        await Unsubscribe(ChatClient, UserId);
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
    #endregion

    #region gRPC Server Communication
    private async Task Subscribe(Chat.ChatClient client, int userId)
    {
        tokenSource = new CancellationTokenSource();
        var sub = client.Subscribe(new Request
        {
            Id = UserId,
            SessionId = this.SessionId
        },
        cancellationToken: tokenSource.Token);

        try
        {
            await foreach (var response in sub.ResponseStream.ReadAllAsync())
            {
                ProcessResponseMessage(response);
            }
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.Unavailable)
        {
            MessageBox.Show($"Lost the Connection to the server...closing Application...\nStatusmessage: {ex.StatusCode}", "Connection lost");
            App.Current.Dispatcher.Invoke(() => { Application.Current.Shutdown(); });
        }
        catch (Exception ex)
        {
            //MessageBox.Show();
        }

        await client.UnsubscribeAsync(new Request { Id = userId });
    }

    private Task Unsubscribe(Chat.ChatClient client, int userId)
    {
        tokenSource.Cancel();
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
                Application.Current.Dispatcher.Invoke(() => ChatsCollectionView.Refresh());
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
    }
    #endregion
    private void MainWindow_Closing(object? sender, CancelEventArgs e)
    {
        Unsubscribe(ChatClient, UserId).Wait();
    }
}