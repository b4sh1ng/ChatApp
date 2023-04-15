using Grpc.Core;
using GrpcServer;
using GrpcServer.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;
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
            subscribers[request.Id] = responseStream;
            while (subscribers.ContainsKey(request.Id))
            {
                var message = await buffer.ReceiveAsync();
                foreach (var serverStreamWriter in subscribers.Values)
                {
                    if (message.NewMessage.ToId == request.Id)
                    {
                        await serverStreamWriter.WriteAsync(message);
                    }
                }
            }
        }
        public override Task<Empty> Unsubscribe(Request request, ServerCallContext context)
        {
            subscribers.Remove(request.Id);
            return Task.FromResult(new Empty());
        }
        public override Task<Empty> PostMessage(Msg request, ServerCallContext context)
        {
            logger.LogInformation($"Nachricht für Chat {request.ChatId} mit Inhalt:\n\"{request.Text}\"");
            var query = dbcontext.Chats
                .Where(c => c.ChatId == request.ChatId)
                .Select(c => c.UserId);

            foreach (var userId in query)
            {
                if (subscribers.ContainsKey(userId))
                {
                    buffer.Post(new SubscriberResponse()
                    {
                        MessageType = 2,
                        NewMessage = new NewMessage()
                        {
                            ToChatId = request.ChatId,
                            FromId = request.FromId,
                            ToId = userId,
                            Text = request.Text,                            
                        }
                    });
                }
            }
            // ...Write Data to DB
            return Task.FromResult(new Empty());
        }
        public override async Task<UserDataResponse> GetUserData(Login request, ServerCallContext context)
        {
            // Später Login Kontrolle + JWT Token hinzufügen
            var DbRequest = dbcontext.Usercredentials.FirstOrDefault(x => x.Username == request.LoginMail);
            return await Task.FromResult(new UserDataResponse
            {
                MyUserid = DbRequest!.UserId,
                MyUsername = DbRequest.Username,
                MyUsernameId = DbRequest.UsernameId,
                MyProfileImgB64 = DbRequest.ProfileImgB64
            });
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
                            B64Img = userdata.ProfileImgB64
                        };

            var result = await query.ToListAsync();

            foreach (var chats in query)
            {
                await responseStream.WriteAsync(new GetChatDataResponse
                {
                    ChatId = chats.ChatId,
                    IsListed = Convert.ToBoolean(chats.IsListed),
                    ChatImgB64 = chats.B64Img,
                });
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
    }
}
