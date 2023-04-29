using Grpc.Core;
using GrpcServer.Entities;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Collections.Concurrent;
using System.Threading.Tasks.Dataflow;
using System.Windows;

namespace GrpcServer.Services;

public class ChatService : Chat.ChatBase
{
    private readonly ILogger<ChatService> logger;
    private readonly TalkzContext dbcontext;
    private static ConcurrentDictionary<int, IServerStreamWriter<SubscriberResponse>> subscribers = new();
    private static readonly BufferBlock<SubscriberResponse> buffer = new();

    public ChatService(ILogger<ChatService> logger, TalkzContext DBContext)
    {
        this.logger = logger;
        dbcontext = DBContext!;
        Task.Run(async () => await SubSender());
    }
    private async Task SubSender()
    {
        while (true)
        {
            var message = await buffer.ReceiveAsync();
            Parallel.ForEach(subscribers, (subscriber) =>
            {
                if (message.ToId == subscriber.Key)
                {
                    logger.LogInformation($"[{DateTime.Now:H:mm:ss:FFF}] Daten senden an ID: {message.ToId}");
                    subscriber.Value.WriteAsync(message);
                }
            });
        }
    }
    //public override async Task Subscribe(Request request, IServerStreamWriter<SubscriberResponse> responseStream, ServerCallContext context)
    //{
    //    logger.LogInformation($"[{DateTime.Now:H:mm:ss:FFF}] Subcount: {subscribers.Count()}");
    //    if (IsSessionNotOk(request.Id, request.SessionId).Result) return;
    //    logger.LogInformation($"[{DateTime.Now:H:mm:ss:FFF}] Subscribe Anfrage erhalten von: {request.Id}");
    //    subscribers.TryAdd(request.Id, responseStream);
    //    var setUserStatus = await dbcontext.Usercredentials.SingleAsync(x => x.UserId == request.Id);
    //    setUserStatus.CurrentStatus = setUserStatus.LastStatus;
    //    await dbcontext.SaveChangesAsync();
    //    var dbRequest = await dbcontext.Friendlists
    //        .Where(x => x.UserId1 == request.Id || x.UserId2 == request.Id)
    //        .ToListAsync();
    //    foreach (var friends in dbRequest)
    //    {
    //        int friendId;
    //        if (friends.UserId1 == request.Id && friends.IsFriend == true)
    //            friendId = friends.UserId2;
    //        else if (friends.UserId2 == request.Id)
    //            friendId = friends.UserId1;
    //        else
    //            continue;
    //        if (subscribers.ContainsKey(friendId))
    //        {
    //            buffer.Post(new SubscriberResponse()
    //            {
    //                MessageType = 4,
    //                ToId = friendId,
    //                NewUserStatus = new NewUserStatus()
    //                {
    //                    UserId = request.Id,
    //                    UserStatus = setUserStatus.CurrentStatus,
    //                }
    //            });
    //        }
    //    }
    //    try
    //    {
    //        while (subscribers.ContainsKey(request.Id) || !context.CancellationToken.IsCancellationRequested)
    //        {
    //            await Task.Delay(1);
    //            // runs for each client, SubSender handles buffer messages
    //        }
    //    }
    //    catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
    //    {
    //        logger.LogWarning($"Verbindung zu Client {request.Id} beendet.");
    //        subscribers.TryRemove(request.Id, out _);
    //    }
    //    catch (Exception ex)
    //    {
    //        logger.LogError(ex.Message);
    //    }
    //    subscribers.TryRemove(request.Id, out _);
    //    setUserStatus.CurrentStatus = 0;
    //    dbRequest = await dbcontext.Friendlists
    //        .Where(x => x.UserId1 == request.Id || x.UserId2 == request.Id)
    //        .ToListAsync();
    //    foreach (var friends in dbRequest)
    //    {
    //        int friendId;
    //        if (friends.UserId1 == request.Id && friends.IsFriend == true)
    //            friendId = friends.UserId2;
    //        else if (friends.UserId2 == request.Id)
    //            friendId = friends.UserId1;
    //        else
    //            continue;
    //        if (subscribers.ContainsKey(friendId))
    //        {
    //            buffer.Post(new SubscriberResponse()
    //            {
    //                MessageType = 4,
    //                ToId = friendId,
    //                NewUserStatus = new NewUserStatus()
    //                {
    //                    UserId = request.Id,
    //                    UserStatus = 0,
    //                }
    //            });
    //        }
    //    }
    //    await dbcontext.SaveChangesAsync();
    //    logger.LogInformation($"[{DateTime.Now:H:mm:ss:FFF}] Stream mit Id: {request.Id} beendet");
    //}
    public override async Task Subscribe(Request request, IServerStreamWriter<SubscriberResponse> responseStream, ServerCallContext context)
    {
        logger.LogInformation($"[{DateTime.Now:H:mm:ss:FFF}] Subcount: {subscribers.Count()}");
        if (await IsSessionNotOk(request.Id, request.SessionId)) return;
        logger.LogInformation($"[{DateTime.Now:H:mm:ss:FFF}] Subscribe Anfrage erhalten von: {request.Id}");
        subscribers.TryAdd(request.Id, responseStream);
        await SetUserStatus(request.Id);
        await SendUserStatusToFriends(request.Id);
        try
        {
            while (subscribers.ContainsKey(request.Id) || !context.CancellationToken.IsCancellationRequested)
            {
                await Task.Delay(1);
            }
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
        {
            logger.LogWarning($"Verbindung zu Client {request.Id} beendet.");
            subscribers.TryRemove(request.Id, out _);
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
        }
        subscribers.TryRemove(request.Id, out _);
        await SetUserStatusOffline(request.Id);
        await SendUserStatusToFriends(request.Id);
        logger.LogInformation($"[{DateTime.Now:H:mm:ss:FFF}] Stream mit Id: {request.Id} beendet");
    }
    public override Task<Empty> Unsubscribe(Request request, ServerCallContext context)
    {
        if (IsSessionNotOk(request.Id, request.SessionId).Result) return Task.FromResult(new Empty());
        subscribers.TryRemove(request.Id, out _);
        logger.LogInformation($"[{DateTime.Now:H:mm:ss:FFF}] Unsubscribe Anfrage erhalten von: {request.Id}");
        return Task.FromResult(new Empty());
    }
    //public override Task<Empty> PostMessage(Msg request, ServerCallContext context)
    //{
    //    if (IsSessionNotOk(request.FromId, request.SessionId).Result) return Task.FromResult(new Empty());
    //    var query = dbcontext.Chats
    //        .Where(c => c.ChatId == request.ChatId)
    //        .Select(c => c.UserId);
    //    var userQuery = dbcontext.Usercredentials
    //        .Single(x => x.UserId == request.FromId);

    //    foreach (var userId in query)
    //    {
    //        if (subscribers.TryGetValue(userId, out var responseStream))
    //        {
    //            logger.LogInformation($"[{DateTime.Now:H:mm:ss:FFF}] Nachrich im Buffer hinzugefügt für ID {userId}");
    //            buffer.Post(new SubscriberResponse()
    //            {
    //                MessageType = 2,
    //                ToId = userId,
    //                NewMessage = new NewMessage()
    //                {
    //                    //Server data
    //                    ToChatId = request.ChatId,
    //                    //User data
    //                    Username = userQuery.Username,
    //                    FromId = request.FromId,
    //                    ImageSource = userQuery.ProfileImgB64,
    //                    Text = request.Text,
    //                    Time = DateTimeOffset.Now.ToUnixTimeSeconds(),
    //                }
    //            });
    //        }
    //    }
    //    dbcontext.Chats.Where(c => c.ChatId == request.ChatId).Single(c => c.UserId != request.FromId).IsListed = true;
    //    dbcontext.Messages.Add(new Message()
    //    {
    //        ChatId = request.ChatId,
    //        Message1 = request.Text,
    //        MessageTimestamp = DateTimeOffset.Now.ToUnixTimeSeconds(),
    //        IsEdited = false,
    //        IsRead = false,
    //        FromId = request.FromId,
    //    });
    //    dbcontext.SaveChangesAsync().Wait();
    //    return Task.FromResult(new Empty());
    //}
    public override async Task<Empty> PostMessage(Msg request, ServerCallContext context)
    {
        if (await IsSessionNotOk(request.FromId, request.SessionId)) return new Empty();
        var userIds = dbcontext.Chats.Where(c => c.ChatId == request.ChatId).Select(c => c.UserId);
        var user = await dbcontext.Usercredentials.SingleOrDefaultAsync(x => x.UserId == request.FromId);

        foreach (var userId in userIds)
        {
            if (subscribers.ContainsKey(userId))
            {
                logger.LogInformation($"[{DateTime.Now:H:mm:ss:FFF}] Nachricht im Buffer hinzugefügt für ID {userId}");
                buffer.Post(new SubscriberResponse()
                {
                    MessageType = 2,
                    ToId = userId,
                    NewMessage = new NewMessage()
                    {
                        //Server data
                        ToChatId = request.ChatId,
                        //User data
                        Username = user.Username,
                        FromId = request.FromId,
                        ImageSource = user.ProfileImgB64,
                        Text = request.Text,
                        Time = DateTimeOffset.Now.ToUnixTimeSeconds(),
                    }
                });
            }
        }
        dbcontext.Chats.Where(c => c.ChatId == request.ChatId).Single(c => c.UserId != request.FromId).IsListed = true;
        dbcontext.Messages.Add(new Message()
        {
            ChatId = request.ChatId,
            Message1 = request.Text,
            MessageTimestamp = DateTimeOffset.Now.ToUnixTimeSeconds(),
            IsEdited = false,
            IsRead = false,
            FromId = request.FromId,
        });
        await dbcontext.SaveChangesAsync();
        return new Empty();
    }
    //public override async Task<Empty> PostNewStatus(NewUserStatus request, ServerCallContext context)
    //{
    //    if (IsSessionNotOk(request.UserId, request.SessionId).Result) return new Empty();
    //    //change status from request, add message to buffer to send to all users he is befriended
    //    var statusRequest = await dbcontext.Usercredentials.SingleOrDefaultAsync(x => x.UserId == request.UserId);
    //    if (statusRequest != null && statusRequest.LastStatus != request.UserStatus)
    //    {
    //        statusRequest.LastStatus = request.UserStatus;
    //        statusRequest.CurrentStatus = request.UserStatus;
    //        logger.LogInformation($"[{DateTime.Now:H:mm:ss:FFF}] Ändere Status von User {request.UserId} zu Status: {request.UserStatus}");
    //        await dbcontext.SaveChangesAsync();
    //        var friendRequest = await dbcontext.Friendlists
    //            .Where(x => x.UserId1 == request.UserId || x.UserId2 == request.UserId)
    //            .ToListAsync();
    //        foreach (var friends in friendRequest)
    //        {
    //            int friendId;
    //            if (friends.UserId1 == request.UserId)
    //                friendId = friends.UserId2;
    //            else
    //                friendId = friends.UserId1;
    //            if (subscribers.ContainsKey(friendId))
    //            {
    //                buffer.Post(new SubscriberResponse()
    //                {
    //                    MessageType = 4,
    //                    ToId = friendId,
    //                    NewUserStatus = new NewUserStatus()
    //                    {
    //                        UserId = request.UserId,
    //                        UserStatus = statusRequest.CurrentStatus,
    //                    }
    //                });
    //            }
    //        }
    //    }
    //    return new Empty();
    //}
    public override async Task<Empty> PostNewStatus(NewUserStatus request, ServerCallContext context)
    {
        if (await IsSessionNotOk(request.UserId, request.SessionId)) return new Empty();

        var user = await dbcontext.Usercredentials.SingleOrDefaultAsync(x => x.UserId == request.UserId);

        if (user?.LastStatus != request.UserStatus)
        {
            user.LastStatus = request.UserStatus;
            user.CurrentStatus = request.UserStatus;

            logger.LogInformation($"[{DateTime.Now:H:mm:ss:FFF}] Ändere Status von User {request.UserId} zu Status: {request.UserStatus}");

            await dbcontext.SaveChangesAsync();

            var friendIds = await dbcontext.Friendlists
                .Where(x => x.UserId1 == request.UserId || x.UserId2 == request.UserId)
                .SelectMany(x => new[] { x.UserId1, x.UserId2 })
                .ToListAsync();

            foreach (var friendId in friendIds)
            {
                if (subscribers.ContainsKey(friendId))
                {
                    buffer.Post(new SubscriberResponse()
                    {
                        MessageType = 4,
                        ToId = friendId,
                        NewUserStatus = new NewUserStatus()
                        {
                            UserId = request.UserId,
                            UserStatus = user.CurrentStatus,
                        }
                    });
                }
            }
        }
        return new Empty();
    }
    public override async Task<IsSuccess> PostFriendRequest(FriendRequestSearch request, ServerCallContext context)
    {
        if (IsSessionNotOk(request.UserId, request.SessionId).Result) return new IsSuccess() { IsOk = false };
        int searchId;
        string[] searchUser = request.SearchTerm.Split('#');
        try
        {
            if (int.TryParse(searchUser[1], out int number))
            {
                searchId = number;
            }
            else
            {
                return new IsSuccess() { IsOk = false };
            }
        }
        catch
        {
            return new IsSuccess() { IsOk = false };
        }
        //Looking for User with Username and his UsernameID => if not exist return false
        var searchRequest = await dbcontext.Usercredentials.SingleOrDefaultAsync(x => x.Username == searchUser[0] && x.UsernameId == searchId);
        if (searchRequest == null)
        {
            return new IsSuccess() { IsOk = false };
        }
        // Getting all friends of the requester
        var getFriends = await dbcontext.Friendlists.Where(x => x.UserId1 == request.UserId || x.UserId2 == request.UserId).ToListAsync();
        Usercredential? requesterData = null;
        foreach (var friend in getFriends)
        {
            int friendId;
            if (friend.UserId1 == request.UserId)
                friendId = friend.UserId2;
            else
                friendId = friend.UserId1;
            if (friendId == searchRequest.UserId)
            {
                return new IsSuccess() { IsOk = false };
            }
        }
        requesterData = await dbcontext.Usercredentials.SingleOrDefaultAsync(x => x.UserId == request.UserId);
        //if (requesterData == null) return new IsSuccess() { IsOk = false };
        await dbcontext.Friendlists.AddAsync(new Friendlist
        {
            IsFriend = false,
            UserId1 = request.UserId,
            UserId2 = searchRequest.UserId,
        });
        await dbcontext.SaveChangesAsync();
        buffer.Post(new SubscriberResponse()
        {
            MessageType = 3,
            ToId = searchRequest.UserId,
            NewRequest = new NewRequest()
            {
                RequestData = new GetFriendDataResponse()
                {
                    FriendId = requesterData.UserId,
                    IsFriend = false,
                    FriendImgB64 = requesterData.ProfileImgB64,
                    FriendUsername = requesterData.Username,
                    FriendUserId = requesterData.UsernameId,
                }
            }
        });
        return new IsSuccess() { IsOk = true };
    }
    public override async Task<UserDataResponse> GetUserData(Request request, ServerCallContext context)
    {
        if (IsSessionNotOk(request.Id, request.SessionId).Result) return new UserDataResponse();
        var userDataRequest = await dbcontext.Usercredentials.FirstOrDefaultAsync(x => x.UserId == request.Id);
        logger.LogInformation($"[{DateTime.Now:H:mm:ss:FFF}] Daten gesendet: {userDataRequest?.Username}");
        var response = (new UserDataResponse
        {
            MyUsername = userDataRequest.Username,
            MyUsernameId = userDataRequest.UsernameId,
            MyProfileImgB64 = userDataRequest.ProfileImgB64,
            MyUserStatus = userDataRequest.LastStatus,
        });
        return response;
    }
    public override async Task GetUserChats(Request request, IServerStreamWriter<GetChatDataResponse> responseStream, ServerCallContext context)
    {
        if (IsSessionNotOk(request.Id, request.SessionId).Result) return;
        var query = from chats in dbcontext.Chats
                    join userdata in dbcontext.Usercredentials
                    on chats.UserId equals userdata.UserId
                    where chats.UserId != request.Id
                    join chatdata in dbcontext.Chats
                    on chats.ChatId equals chatdata.ChatId
                    where chatdata.UserId == request.Id
                    select new
                    {
                        chats.ChatId,
                        chats.IsListed,
                        B64Img = userdata.ProfileImgB64,
                        ChatName = userdata.Username,
                        userdata.CurrentStatus,
                    };
        var result = await query.ToListAsync();
        foreach (var chats in result)
        {
            await responseStream.WriteAsync(new GetChatDataResponse
            {
                ChatId = chats.ChatId,
                IsListed = Convert.ToBoolean(await dbcontext.Chats.Where(x => x.UserId == request.Id).Where(x => x.ChatId == chats.ChatId).Select(x => x.IsListed).SingleAsync()),
                ChatImgB64 = chats.B64Img,
                ChatName = chats.ChatName,
                CurrentStatus = chats.CurrentStatus,
            });
            logger.LogInformation($"[{DateTime.Now:H:mm:ss:FFF}] Information von ChatId {chats.ChatId} gesendet.");
        }
    }
    public override async Task GetChatData(ChatDataRequest request, IServerStreamWriter<MessageToChat> responseStream, ServerCallContext context)
    {
        if (IsSessionNotOk(request.UserId, request.SessionId).Result) return;
        var query = from message in dbcontext.Messages
                    where message.ChatId == request.ChatId
                    join user in dbcontext.Usercredentials
                    on message.FromId equals user.UserId
                    select new
                    {
                        Username = user.Username,
                        ImageSource = user.ProfileImgB64,
                        FromId = message.FromId,
                        Text = message.Message1,
                        Time = message.MessageTimestamp,
                        IsEdited = Convert.ToBoolean(message.IsEdited),
                        IsRead = Convert.ToBoolean(message.IsRead),
                    };
        var result = await query.ToListAsync();
        foreach (var message in result)
        {
            await responseStream.WriteAsync(new MessageToChat
            {
                Username = message.Username,
                ImageSource = message.ImageSource,
                FromId = message.FromId,
                Text = message.Text,
                Time = message.Time,
                IsEdited = message.IsEdited,
                IsRead = message.IsRead,
            });
            logger.LogInformation($"[{DateTime.Now:H:mm:ss:FFF}] {message.Username} = {message.Text}");
        }
    }
    public override async Task<NewChat> GetChatId(ChatRequest request, ServerCallContext context)
    {
        if (IsSessionNotOk(request.UserId, request.SessionId).Result) return new NewChat();
        logger.LogInformation($"Anfrage von ClientID {request.UserId} erhalten");
        //Get current ChatId if exist
        var queryChatId = dbcontext.Chats
            .Where(u1 => u1.UserId == request.UserId)
            .Join(dbcontext.Chats.Where(u2 => u2.UserId == request.FriendId), u1 => u1.ChatId, u2 => u2.ChatId, (u1, u2) => u1.ChatId)
            .SingleOrDefault();
        //create new Chat&ChatId if no ChadId exist(if queryChatid is 0)
        if (queryChatId is 0)
        {
            int maxChatId = 0;
            if (dbcontext.Chats.Sum(x => x.ChatId) != 0)
            {
                maxChatId = dbcontext.Chats.Max(c => c.ChatId);
            }
            int newChatId = maxChatId + 1;
            await dbcontext.Chats.AddAsync(new Entities.Chat
            {
                ChatId = newChatId,
                IsListed = true,
                UserId = request.UserId,
            });
            await dbcontext.Chats.AddAsync(new Entities.Chat
            {
                ChatId = newChatId,
                IsListed = false,
                UserId = request.FriendId,
            });
            await dbcontext.SaveChangesAsync();
            // Send new ChatData to Requester
            buffer.Post(new SubscriberResponse
            {
                MessageType = 1,
                ToId = request.UserId,
                NewChat = new NewChat()
                {
                    ChatData = new GetChatDataResponse()
                    {
                        ChatId = newChatId,
                        ChatImgB64 = await dbcontext.Usercredentials.Where(x => x.UserId == request.FriendId).Select(x => x.ProfileImgB64).SingleAsync(),
                        ChatName = await dbcontext.Usercredentials.Where(x => x.UserId == request.FriendId).Select(x => x.Username).SingleAsync(),
                        IsListed = true,
                    }
                }
            });
            // Send new ChatData to Friend but its not listed for him in his UI until Requester sends a Message
            buffer.Post(new SubscriberResponse
            {
                MessageType = 1,
                ToId = request.FriendId,
                NewChat = new NewChat()
                {
                    ChatData = new GetChatDataResponse()
                    {
                        ChatId = newChatId,
                        ChatImgB64 = await dbcontext.Usercredentials.Where(x => x.UserId == request.UserId).Select(x => x.ProfileImgB64).SingleAsync(),
                        ChatName = await dbcontext.Usercredentials.Where(x => x.UserId == request.UserId).Select(x => x.Username).SingleAsync(),
                        IsListed = false,
                    }
                }
            });
            logger.LogInformation($"Neuen Chat erstellt mit ID {newChatId}");
            return new NewChat()
            {
                ChatData = new GetChatDataResponse() { ChatId = newChatId }
            };
        }
        var chat = dbcontext.Chats.FirstOrDefault(x => x.ChatId == queryChatId && x.UserId == request.UserId);
        chat.IsListed = true;
        dbcontext.SaveChanges();
        return new NewChat() { ChatData = new GetChatDataResponse() { ChatId = queryChatId } };
    }
    public override async Task GetUserFriends(Request request, IServerStreamWriter<GetFriendDataResponse> responseStream, ServerCallContext context)
    {
        if (IsSessionNotOk(request.Id, request.SessionId).Result) return;
        var dbRequest = await dbcontext.Friendlists
            .Where(x => x.UserId1 == request.Id || x.UserId2 == request.Id)
            .ToListAsync();
        foreach (var friends in dbRequest)
        {
            int friendId;

            if (friends.UserId1 == request.Id && friends.IsFriend == true)
                friendId = friends.UserId2;
            else if (friends.UserId2 == request.Id)
                friendId = friends.UserId1;
            else
                continue;
            var friendDataRequest = await dbcontext.Usercredentials.Where(x => x.UserId == friendId).SingleAsync();
            await responseStream.WriteAsync(new GetFriendDataResponse
            {
                FriendId = friendId,
                IsFriend = Convert.ToBoolean(friends.IsFriend),
                FriendImgB64 = friendDataRequest.ProfileImgB64,
                FriendUsername = friendDataRequest.Username,
                FriendUserId = friendDataRequest.UserId,
                CurrentStatus = friendDataRequest.CurrentStatus,
            });
        }
    }
    public override async Task<Empty> FriendRequesting(FriendRequest request, ServerCallContext context)
    {
        if (IsSessionNotOk(request.UserId, request.SessionId).Result) return new Empty();
        var friendRequest = await dbcontext.Friendlists.SingleOrDefaultAsync(x => x.UserId1 == request.FriendId && x.UserId2 == request.UserId);
        if (request.IsAccepted == true)
        {
            if (friendRequest != null)
            {
                friendRequest.IsFriend = true;
                var friendDataRequest = await dbcontext.Usercredentials.SingleAsync(x => x.UserId == request.UserId);
                buffer.Post(new SubscriberResponse
                {
                    MessageType = 3,
                    ToId = request.FriendId,
                    NewRequest = new NewRequest()
                    {
                        RequestData = new GetFriendDataResponse()
                        {
                            FriendId = request.UserId,
                            FriendUsername = friendDataRequest?.Username,
                            FriendUserId = friendDataRequest.UserId,
                            FriendImgB64 = friendDataRequest?.ProfileImgB64,
                            CurrentStatus = friendDataRequest.CurrentStatus,
                            IsFriend = true,
                        }
                    }
                });
            }
        }
        else
        {
            if (friendRequest != null)
            {
                dbcontext.Friendlists.Remove(friendRequest);
            }
        }
        await dbcontext.SaveChangesAsync();
        return new Empty();
    }
    public override async Task<IsSuccess> DeleteFriend(FriendRequest request, ServerCallContext context)
    {
        if (IsSessionNotOk(request.UserId, request.SessionId).Result) return new IsSuccess() { IsOk = false };
        var deleteRequest = await dbcontext.Friendlists
            .SingleAsync(x => (x.UserId1 == request.UserId && x.UserId2 == request.FriendId) || (x.UserId1 == request.FriendId && x.UserId2 == request.UserId));
        dbcontext.Friendlists.Remove(deleteRequest);
        await dbcontext.SaveChangesAsync();
        buffer.Post(new SubscriberResponse()
        {
            MessageType = 5,
            ToId = request.FriendId,
            RemoveFriend = new RemoveFriend()
            {
                FriendId = request.UserId
            }
        });
        return new IsSuccess() { IsOk = true };
        // add buffer message to delete friend from both clients if connected
    }
    private async Task<bool> IsSessionNotOk(int userId, string sessionId)
    {
        var sessionCheckRequest = await dbcontext.Usersessions.SingleOrDefaultAsync(x => x.UserId == userId && x.SessionId == sessionId);
        if (sessionCheckRequest == null || sessionCheckRequest.IsExpired)
            return true;
        return false;
    }
    private async Task SetUserStatus(int id)
    {
        var setUserStatus = await dbcontext.Usercredentials.SingleOrDefaultAsync(x => x.UserId == id);
        setUserStatus.CurrentStatus = setUserStatus.LastStatus;
        await dbcontext.SaveChangesAsync();
    }
    private async Task SetUserStatusOffline(int id)
    {
        var setUserStatus = await dbcontext.Usercredentials.SingleOrDefaultAsync(x => x.UserId == id);
        setUserStatus.CurrentStatus = 0;
        await dbcontext.SaveChangesAsync();
    }
    private async Task SendUserStatusToFriends(int id)
    {
        var friendIds = await dbcontext.Friendlists
            .Where(x => x.UserId1 == id || x.UserId2 == id)
            .Where(x => x.IsFriend == true)
            .Select(x => x.UserId1 == id ? x.UserId2 : x.UserId1)
            .ToListAsync();

        foreach (var friendId in friendIds)
        {
            if (subscribers.ContainsKey(friendId))
            {
                buffer.Post(new SubscriberResponse()
                {
                    MessageType = 4,
                    ToId = friendId,
                    NewUserStatus = new NewUserStatus()
                    {
                        UserId = id,
                        UserStatus = dbcontext.Usercredentials.SingleOrDefault(x => x.UserId == id)?.CurrentStatus ?? 0,
                    }
                });
            }
        }
    }
}
