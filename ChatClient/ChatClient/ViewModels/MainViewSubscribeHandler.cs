﻿using ChatClient.Models;
using GrpcServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ChatClient.ViewModels;

public partial class MainView
{
    bool ProcessResponseMessage(SubscriberResponse response) => response switch
    {
        { MessageType: 1 } => ProcessNewChat(response.NewChat.NewChatId),
        { MessageType: 2 } => ProcessNewMessage(response),
        { MessageType: 3 } => ProcessNewFriendRequest(response.NewRequest.RequestData.FriendId),
        { MessageType: 4 } => ProcessNewUserStatus(response.NewUserStatus.UserId, response.NewUserStatus.UserStatus),
        { MessageType: 5 } => ProcessFriendRemoved(response.RemoveFriend.FriendId),
        _ => throw new NotImplementedException("Unexpected message type received.")
    };
    bool ProcessNewChat(int chatId)
    {
        //try
        //{
        //    Console.WriteLine($"Neuer Chat mit der Id {chatId}");
        //}
        //catch (Exception ex)
        //{
        //    Console.WriteLine(ex);
        //}
        return true;
    }

    bool ProcessNewMessage(SubscriberResponse response)
    {
        var resp = response.NewMessage;
        try
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Chats
                    .FirstOrDefault(x => x.ChatId == resp.ToChatId).Messages
                    .Add(new MessageModel()
                    {
                        Username = resp.Username,
                        FromId = resp.FromId,
                        ImageSource = resp.ImageSource,
                        Message = resp.Text,
                        Time = DateTimeOffset.FromUnixTimeSeconds(resp.Time).DateTime,                        
                    });
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
        return true;
    }

    bool ProcessNewFriendRequest(int friendId)
    {
        //Console.WriteLine($"Neuer Freundesanfrage erhalten mit der Id {friendId}");
        return true;
    }

    bool ProcessNewUserStatus(int userId, int userStatus)
    {
        //Console.WriteLine($"Neuen Status für Nutzer {userId} erhalten mit dem Status {userStatus}");
        return true;
    }

    bool ProcessFriendRemoved(int friendId)
    {
        //Console.WriteLine($"Freund gelöscht mit der ID: {friendId}");
        return true;
    }
}
