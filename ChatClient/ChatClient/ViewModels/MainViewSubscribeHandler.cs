using ChatClient.Enums;
using ChatClient.Models;
using GrpcServer;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace ChatClient.ViewModels;

public partial class MainView
{
    bool ProcessResponseMessage(SubscriberResponse response) => response switch
    {
        { MessageType: 1 } => ProcessNewChat(response),
        { MessageType: 2 } => ProcessNewMessage(response),
        { MessageType: 3 } => ProcessNewFriendRequest(response),
        { MessageType: 4 } => ProcessNewUserStatus(response.NewUserStatus.UserId, response.NewUserStatus.UserStatus),
        { MessageType: 5 } => ProcessFriendRemoved(response.RemoveFriend.FriendId),
        _ => throw new NotImplementedException("Unexpected message type received.")
    };
    bool ProcessNewChat(SubscriberResponse response)
    {
        try
        {
            Application.Current.Dispatcher.Invoke(() =>
                {
                    Chats?.Add(new ChatModel()
                    {
                        ChatId = response.NewChat.ChatData.ChatId,
                        ChatName = response.NewChat.ChatData?.ChatName,
                        ImageSource = response.NewChat.ChatData?.ChatImgB64,
                        IsChatListed = response.NewChat.ChatData!.IsListed,
                        Messages = new ObservableCollection<MessageModel>(),
                    });
                });
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
        taskCompletion?.SetResult(true);
        return true;
    }
    bool ProcessNewMessage(SubscriberResponse response)
    {
        var resp = response.NewMessage;
        try
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Chats?
                    .FirstOrDefault(x => x.ChatId == resp.ToChatId)?.Messages?
                    .Add(new MessageModel()
                    {
                        Username = resp.Username,
                        FromId = resp.FromId,
                        ImageSource = resp.ImageSource,
                        Message = resp.Text,
                        Time = DateTimeOffset.FromUnixTimeSeconds(resp.Time).DateTime,
                    });
                Chats.Single(x => x.ChatId == resp.ToChatId).IsChatListed = true;
                ChatsCollectionView.Refresh();
            });
            // Update ChatViewCollection
            
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
        return true;
    }

    bool ProcessNewFriendRequest(SubscriberResponse response)
    {
        FriendList.Add(new FriendModel()
        {
            Username = response.NewRequest.RequestData.FriendUsername,
            UsernameId = response.NewRequest.RequestData.FriendUserId,
            FriendId = response.NewRequest.RequestData.FriendId,
            ImageSource = response.NewRequest.RequestData.FriendImgB64,
            IsFriend = response.NewRequest.RequestData.IsFriend,
        });
        // Update CollectionViewSource... HOW?!?!?      
        return true;
    }
    bool ProcessNewUserStatus(int userId, int userStatus)
    {
        try
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var getChatId = ChatClient.GetChatId(new ChatRequest
                {
                    FriendId = userId,
                    SessionId = SessionId,
                    UserId = this.UserId
                });
                var changeUserChatStatus = Chats.Single(x => x.ChatId == getChatId.ChatData.ChatId);
                changeUserChatStatus.CurrentStatus = StatusEnumHandler.GetStatusColor((State)userStatus);

                var changeFriendListStatus = FriendList.Single(x => x.FriendId == userId);
                changeFriendListStatus.CurrentStatus = StatusEnumHandler.GetStatusColor((State)userStatus);
                // Update ViewCollection...
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
        return true;
    }
    bool ProcessFriendRemoved(int friendId)
    {
        var friendToRemove = FriendList.Single(x => x.FriendId == friendId);
        FriendList.Remove(friendToRemove);
        return true;
    }
}
