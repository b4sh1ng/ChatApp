using Grpc.Core;
using GrpcServer;
using GrpcServer.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace GrpcServer.Services
{
    public class ChatService : Chat.ChatBase
    {
        private static Dictionary<int, ServerCallContext> Subscribers = new();
        private readonly ILogger<ChatService> _logger;
        private readonly TalkzContext _dbcontext;
        private static Msg? message;
        public ChatService(ILogger<ChatService> logger, TalkzContext DBContext)
        {
            _logger = logger;
            _dbcontext = DBContext!;
        }

        //public override async Task Subscribe(Request request, IServerStreamWriter<SubscriberResponse> responseStream, ServerCallContext context)
        //{
        //    //KeyValuePair<int, ServerCallContext> foundPair = Subscribers.FirstOrDefault(x => x.Value == context);
        //    ServerCallContext isIdHere = Subscribers.FirstOrDefault(x => x.Key == request.Id).Value;
        //    if (isIdHere is not null)
        //    {
        //        _logger.LogInformation($"Subscribe Anfrage von {request.Id} abgelehnt, da schon vorhanden.");
        //        return;
        //    }
        //    _logger.LogInformation($"Subscribe Anfrage erhalten von: {request.Id}");
        //    Subscribers.Add(request.Id, context);
        //    await responseStream.WriteAsync(new SubscriberResponse { Data = $"Connected to Server!" });
        //    while (!context.CancellationToken.IsCancellationRequested)
        //    {
        //        if (message == null) continue;
        //        if (message.ToId == request.Id)
        //        {
        //            await responseStream.WriteAsync(new SubscriberResponse { Data = message.Text, FromId = message.FromId });
        //            message = null;
        //        }
        //        await Task.Delay(1);
        //    }
        //}
        public override async Task Subscribe(Request request, IServerStreamWriter<SubscriberResponse> responseStream, ServerCallContext context)
        {
            ServerCallContext isIdHere = Subscribers.FirstOrDefault(x => x.Key == request.Id).Value;
            if (isIdHere is not null)
            {
                _logger.LogInformation($"Subscribe Anfrage von {request.Id} abgelehnt, da schon vorhanden.");
                return;
            }
            _logger.LogInformation($"Subscribe Anfrage erhalten von: {request.Id}");
            Subscribers.Add(request.Id, context);
            while (!context.CancellationToken.IsCancellationRequested)
            {

            }

        }
        public override Task<Empty> PostMessage(Msg request, ServerCallContext context)
        {
            _logger.LogInformation($"Nachricht von {request.FromId} erhalten für: {request.ToId}\n Inhalt: {request.Text}");
            message = request;
            return Task.FromResult(new Empty());
        }

        public override async Task<UserDataResponse> GetUserData(Login request, ServerCallContext context)
        {
            // Später Login Kontrolle + JWT Token hinzufügen
            var DbRequest = _dbcontext.Usercredentials.FirstOrDefault(x => x.Username == request.LoginMail);
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
            var query = from chats in _dbcontext.Chats
                        join userdata in _dbcontext.Usercredentials
                        on chats.UserId equals userdata.UserId
                        where chats.UserId != request.Id
                        join chatdata in _dbcontext.Chats
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
            var DbRequest = await _dbcontext.Friendlists
                .Where(x => x.UserId1 == request.Id || x.UserId2 == request.Id)
                .ToListAsync();

            foreach (var friends in DbRequest)
            {
                int friendId;

                if (friends.UserId1 == request.Id)
                    friendId = friends.UserId2;
                else
                    friendId = friends.UserId1;

                var FriendImgRequest = await _dbcontext.Usercredentials.Where(x => x.UserId == friendId).SingleAsync();
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
