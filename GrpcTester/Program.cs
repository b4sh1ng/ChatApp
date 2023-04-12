using Grpc.Core;
using Grpc.Net.Client;
using GrpcServer;

int id;
string? name;
string? pic;
int nameid;

var channel = GrpcChannel.ForAddress("http://localhost:5292");
var client = new Chat.ChatClient(channel);

var response = client.GetUserData(new Login
{
    LoginMail = "Bash",
    Password = "test"
});
id = response.MyUserid;
name = response.MyUsername;
pic = response.MyProfileImgB64;
nameid = response.MyUsernameId;

Console.WriteLine($"User ID: {id}\nUser Name: {name}\nUsername Id: {nameid}");

var chatlist = client.GetUserChats(new Request { Id = id });

await foreach (var chat in chatlist.ResponseStream.ReadAllAsync())
{
    Console.WriteLine($"User {name} hat einen Chat mit der ID: {chat.ChatId} und ist gelistet: {chat.IsListed} mit dem Bild: {chat.ChatImgB64}");
}

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
Console.ReadKey();