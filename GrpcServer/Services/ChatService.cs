using Grpc.Core;
using GrpcServer;
using GrpcServer.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks.Dataflow;

namespace GrpcServer.Services
{
    public class ChatService : Chat.ChatBase
    {
        private readonly ILogger<ChatService> logger;
        private readonly TalkzContext dbcontext;
        private static Dictionary<int, IServerStreamWriter<SubscriberResponse>> subscribers = new();
        private static readonly BufferBlock<SubscriberResponse> buffer = new();

        public ChatService(ILogger<ChatService> logger, TalkzContext DBContext)
        {
            this.logger = logger;
            dbcontext = DBContext!;
        }
        public override async Task Subscribe(Request request, IServerStreamWriter<SubscriberResponse> responseStream, ServerCallContext context)
        {
            logger.LogInformation($"Subscribe Anfrage erhalten von: {request.Id}");
            //var cancellationToken = context.CancellationToken;
            subscribers[request.Id] = responseStream;
            while (subscribers.ContainsKey(request.Id))
            {
                var message = await buffer.ReceiveAsync();
                //if (cancellationToken.IsCancellationRequested)
                //{
                //    subscribers.Remove(request.Id);
                //    logger.LogInformation($"[{DateTime.Now}] Stream mit Id: {request.Id} beendet");
                //    return;
                //}
                foreach (var serverStreamWriter in subscribers.Values)
                {
                    if (message.NewMessage.ToId == request.Id)
                    {
                        logger.LogInformation($"[{DateTime.Now}] Nachricht senden and ID: {request.Id}");
                        await serverStreamWriter.WriteAsync(message);
                    }
                }
            }
            subscribers.Remove(request.Id);
            logger.LogInformation($"[{DateTime.Now}] Stream mit Id: {request.Id} beendet");
        }
        public override Task<Empty> Unsubscribe(Request request, ServerCallContext context)
        {
            subscribers.Remove(request.Id);
            logger.LogInformation($"[{DateTime.Now}] Unsubscribe Anfrage erhalten von: {request.Id}");
            return Task.FromResult(new Empty());
        }
        public override Task<Empty> PostMessage(Msg request, ServerCallContext context)
        {
            logger.LogInformation($"[{DateTime.Now}] Nachricht für Chat {request.ChatId} mit Inhalt:\n\"{request.Text}\"");
            var query = dbcontext.Chats
                .Where(c => c.ChatId == request.ChatId)
                .Select(c => c.UserId);
            var userQuery = dbcontext.Usercredentials
                .Single(x => x.UserId == request.FromId);
                

            foreach (var userId in query)
            {
                if (subscribers.ContainsKey(userId))
                {
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
            // ...Write Data to DB
            return Task.FromResult(new Empty());
        }
        public override async Task<UserDataResponse> GetUserData(Login request, ServerCallContext context)
        {
            // Später Login Kontrolle + JWT Token hinzufügen
            var DbRequest = dbcontext.Usercredentials.FirstOrDefault(x => x.Username == request.LoginMail);
            logger.LogInformation($"[{DateTime.Now}] Daten gesendet: {DbRequest.Username}");
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
                logger.LogInformation($"[{DateTime.Now}] Information von ChatId {chats.ChatId} gesendet.");
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
                logger.LogInformation($"[{DateTime.Now}] {message.Username} = {message.Text}");
            }
        }
    }
}
