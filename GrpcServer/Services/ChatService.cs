using Grpc.Core;
using GrpcServer;
using GrpcServer.Entities;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks.Dataflow;

namespace GrpcServer.Services
{
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
                        logger.LogInformation($"[{DateTime.Now:H:mm:ss:FFF}] Nachricht senden an ID: {message.ToId}");
                        subscriber.Value.WriteAsync(message);
                    }
                });
            }
        }
        public override async Task Subscribe(Request request, IServerStreamWriter<SubscriberResponse> responseStream, ServerCallContext context)
        {
            logger.LogInformation($"[{DateTime.Now:H:mm:ss:FFF}] Subscribe Anfrage erhalten von: {request.Id}");
            subscribers.TryAdd(request.Id, responseStream);

            while (subscribers.ContainsKey(request.Id))
            {
                await Task.Delay(1);
                // runs for each client, SubSender handles buffer messages
            }
            subscribers.TryRemove(request.Id, out _);
            logger.LogInformation($"[{DateTime.Now:H:mm:ss:FFF}] Stream mit Id: {request.Id} beendet");
        }
        public override Task<Empty> Unsubscribe(Request request, ServerCallContext context)
        {
            subscribers.TryRemove(request.Id, out _);
            logger.LogInformation($"[{DateTime.Now:H:mm:ss:FFF}] Unsubscribe Anfrage erhalten von: {request.Id}");
            return Task.FromResult(new Empty());
        }
        public override Task<Empty> PostMessage(Msg request, ServerCallContext context)
        {
            //logger.LogInformation($"[{DateTime.Now}] Nachricht für Chat {request.ChatId} mit Inhalt:\n\"{request.Text}\"");
            var query = dbcontext.Chats
                .Where(c => c.ChatId == request.ChatId)
                .Select(c => c.UserId);
            var userQuery = dbcontext.Usercredentials
                .Single(x => x.UserId == request.FromId);


            foreach (var userId in query)
            {
                if (subscribers.TryGetValue(userId, out var responseStream))
                {
                    logger.LogInformation($"[{DateTime.Now:H:mm:ss:FFF}] Nachrich im Buffer hinzugefügt für ID {userId}");
                    buffer.Post(new SubscriberResponse()
                    {
                        MessageType = 2,
                        ToId = userId,
                        NewMessage = new NewMessage()
                        {
                            //Server data
                            ToChatId = request.ChatId,
                            //User data
                            Username = userQuery.Username,
                            FromId = request.FromId,
                            ImageSource = userQuery.ProfileImgB64,
                            Text = request.Text,
                            Time = DateTimeOffset.Now.ToUnixTimeSeconds(),
                        }
                    });
                }
            }

            dbcontext.Messages.Add(new Message()
            {
                ChatId = request.ChatId,
                Message1 = request.Text,
                MessageTimestamp = DateTimeOffset.Now.ToUnixTimeSeconds(),
                IsEdited = false,
                IsRead = false,
                FromId = request.FromId,
            });
            dbcontext.SaveChangesAsync().Wait();
            return Task.FromResult(new Empty());
        }
        public override async Task<UserDataResponse> GetUserData(Login request, ServerCallContext context)
        {
            // Später Login Kontrolle + JWT Token hinzufügen
            var DbRequest = dbcontext.Usercredentials.FirstOrDefault(x => x.Username == request.LoginMail);
            logger.LogInformation($"[{DateTime.Now:H:mm:ss:FFF}] Daten gesendet: {DbRequest.Username}");
            var response = (new UserDataResponse
            {
                MyUserid = DbRequest!.UserId,
                MyUsername = DbRequest.Username,
                MyUsernameId = DbRequest.UsernameId,
                MyProfileImgB64 = DbRequest.ProfileImgB64,
            });
            return response;
        }
        public override async Task GetUserChats(Request request, IServerStreamWriter<GetChatDataResponse> responseStream, ServerCallContext context)
        {
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
                });
                logger.LogInformation($"[{DateTime.Now:H:mm:ss:FFF}] Information von ChatId {chats.ChatId} gesendet.");
            }
        }
        public override async Task GetChatData(Request request, IServerStreamWriter<MessageToChat> responseStream, ServerCallContext context)
        {
            var query = from message in dbcontext.Messages
                        where message.ChatId == request.Id
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
            logger.LogInformation($"Anfrage von ClientID {request.UserId} erhalten");
            //Get current ChatId if exist
            var queryChatId = dbcontext.Chats
                .Where(u1 => u1.UserId == request.UserId)
                .Join(dbcontext.Chats.Where(u2 => u2.UserId == request.FriendId), u1 => u1.ChatId, u2 => u2.ChatId, (u1, u2) => u1.ChatId)
                .SingleOrDefault();
            //create new Chat&ChatId if no ChadId exist(if queryChatid is 0)
            if (queryChatId is 0)
            {
                var maxChatId = dbcontext.Chats.Max(c => c.ChatId);
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
                // Send new ChatData to Friend but its not listed for him in his UI until Requester sent a Message
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
            var dbRequest = await dbcontext.Friendlists
                .Where(x => x.UserId1 == request.Id || x.UserId2 == request.Id)
                .ToListAsync();

            foreach (var friends in dbRequest)
            {
                int friendId;

                if (friends.UserId1 == request.Id)
                    friendId = friends.UserId2;
                else
                    friendId = friends.UserId1;

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
            var friendRequest = await dbcontext.Friendlists.SingleOrDefaultAsync(x => x.UserId1 == request.UserId && x.UserId2 == request.FriendId);
            if (request.IsAccepted == true)
            {
                if (friendRequest != null)
                {
                    friendRequest.IsFriend = true;
                }
            }
            else
            {
                if (friendRequest != null)
                {
                    dbcontext.Friendlists.Remove(friendRequest);
                }
            }
            dbcontext.SaveChanges();
            return new Empty();
        }
    }
}
