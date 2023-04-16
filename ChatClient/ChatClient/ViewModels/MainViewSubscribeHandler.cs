using GrpcServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatClient.ViewModels;

public partial class MainView
{
    static bool ProcessResponseMessage(SubscriberResponse response) => response switch
    {
        { MessageType: 1 } => ProcessNewChat(response.NewChat.NewChatId),
        { MessageType: 2 } => ProcessNewMessage(response.NewMessage.ToChatId, response.NewMessage.Text),
        { MessageType: 3 } => ProcessNewFriendRequest(response.NewRequest.RequestData.FriendId),
        { MessageType: 4 } => ProcessNewUserStatus(response.NewUserStatus.UserId, response.NewUserStatus.UserStatus),
        { MessageType: 5 } => ProcessFriendRemoved(response.RemoveFriend.FriendId),
        _ => throw new NotImplementedException("Unexpected message type received.")
    };
    static bool ProcessNewChat(int chatId)
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

    static bool ProcessNewMessage(int chatId, string message)
    {
        //Console.WriteLine($"Nachricht erhalten für Chat {chatId} mit Inhalt : {message}");
        return true;
    }

    static bool ProcessNewFriendRequest(int friendId)
    {
        //Console.WriteLine($"Neuer Freundesanfrage erhalten mit der Id {friendId}");
        return true;
    }

    static bool ProcessNewUserStatus(int userId, int userStatus)
    {
        //Console.WriteLine($"Neuen Status für Nutzer {userId} erhalten mit dem Status {userStatus}");
        return true;
    }

    static bool ProcessFriendRemoved(int friendId)
    {
        //Console.WriteLine($"Freund gelöscht mit der ID: {friendId}");
        return true;
    }
}
