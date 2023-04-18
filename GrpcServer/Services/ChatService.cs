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
            while(true)
            {
                var message = await buffer.ReceiveAsync();
                Parallel.ForEach (subscribers, (subscriber) =>
                {
                    if (message.NewMessage.ToId == subscriber.Key)
                    {
                        logger.LogInformation($"[{DateTime.Now:H:mm:ss:FFF}] Nachricht senden an ID: {message.NewMessage.ToId}");
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
                        NewMessage = new NewMessage()
                        {
                            //Server data
                            ToChatId = request.ChatId,
                            ToId = userId,
                            //User data
                            Username = userQuery.Username,
                            FromId = request.FromId,
                            ImageSource = userQuery.ProfileImgB64,
                            Text = request.Text,
                            Time = DateTimeOffset.Now.ToUnixTimeSeconds(),
                        }
                    }); ;
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

            foreach (var chats in query)
            {
                await responseStream.WriteAsync(new GetChatDataResponse
                {
                    ChatId = chats.ChatId,
                    IsListed = Convert.ToBoolean(chats.IsListed),
                    ChatImgB64 = chats.B64Img,
                    ChatName = chats.ChatName,
                });
                logger.LogInformation($"[{DateTime.Now:H:mm:ss:FFF}] Information von ChatId {chats.ChatId} gesendet.");
            }
        }
        public override async Task GetUserFriends(Request request, IServerStreamWriter<GetFriendDataResponse> responseStream, ServerCallContext context)
        {
            var DbRequest = await dbcontext.Friendlists
                .Where(x => x.UserId1 == request.Id || x.UserId2 == request.Id)
                .ToListAsync();

            foreach (var friends in DbRequest)
            {
                int friendId;

                if (friends.UserId1 == request.Id)
                    friendId = friends.UserId2;
                else
                    friendId = friends.UserId1;

                var FriendImgRequest = await dbcontext.Usercredentials.Where(x => x.UserId == friendId).SingleAsync();
                await responseStream.WriteAsync(new GetFriendDataResponse
                {
                    FriendId = friendId,
                    IsFriend = Convert.ToBoolean(friends.IsFriend),
                    FriendImgB64 = FriendImgRequest.ProfileImgB64,
                });
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
    }
}
