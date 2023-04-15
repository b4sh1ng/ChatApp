﻿using Grpc.Core;
using Grpc.Net.Client;
using GrpcServer;

int id;
string? name;
string? pic;
int nameid;

var channel = GrpcChannel.ForAddress("http://localhost:5292");
var client = new Chat.ChatClient(channel);
#region GetUserData
var response = client.GetUserData(new Login
{
    LoginMail = "Preacher",
    Password = "test"
});
id = response.MyUserid;
name = response.MyUsername;
pic = response.MyProfileImgB64;
nameid = response.MyUsernameId;
Console.WriteLine($"User ID: {id}\nUser Name: {name}\nUsername Id: {nameid}");
#endregion
#region GetUserChats
var chatlist = client.GetUserChats(new Request { Id = id });

await foreach (var chat in chatlist.ResponseStream.ReadAllAsync())
{
    Console.WriteLine($"User {name} hat einen Chat mit der ID: {chat.ChatId} und ist gelistet: {chat.IsListed} mit dem Bild: {chat.ChatImgB64}");
}
#endregion
#region GetUserFriends
var friendList = client.GetUserFriends(new Request { Id = id });
await foreach (var friend in friendList.ResponseStream.ReadAllAsync())
{
    if (friend.IsFriend)
    {
        Console.WriteLine($"User {name} hat einen Freund mit der Id: {friend.FriendId} und Bild: {friend.FriendImgB64}");
    }
    else
    {
        Console.WriteLine($"User {name} hat eine Freundesanfrage mit der UserId: {friend.FriendId} und Bild: {friend.FriendImgB64}");
    }
}
#endregion
#region Subscribe
var sub = client.Subscribe(new Request { Id = id });
#endregion
#region PostMessage
await client.PostMessageAsync(new Msg { Text = "Testtext", ChatId = 1, FromId = id });
#endregion
#region GetSubData
using (sub)
{
    var responseReaderTask = Task.Run(async () =>
    {
        while (await sub.ResponseStream.MoveNext())
        {
            ProcessResponseMessage(sub.ResponseStream.Current);
        }
    });
    await responseReaderTask;
}
#endregion
Console.ReadKey();
#region Methods
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
    try
    {
        Console.WriteLine($"Neuer Chat mit der Id {chatId}");
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex);
    }
    return true;
}

static bool ProcessNewMessage(int chatId, string message)
{
    Console.WriteLine($"Nachricht erhalten für Chat {chatId} mit Inhalt : {message}");
    return true;
}

static bool ProcessNewFriendRequest(int friendId)
{
    Console.WriteLine($"Neuer Freundesanfrage erhalten mit der Id {friendId}");
    return true;
}

static bool ProcessNewUserStatus(int userId, int userStatus)
{
    Console.WriteLine($"Neuen Status für Nutzer {userId} erhalten mit dem Status {userStatus}");
    return true;
}

static bool ProcessFriendRemoved(int friendId)
{
    Console.WriteLine($"Freund gelöscht mit der ID: {friendId}");
    return true;
}
#endregion