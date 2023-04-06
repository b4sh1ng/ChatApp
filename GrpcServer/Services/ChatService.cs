using Grpc.Core;
using GrpcServer;
using GrpcServer.Entities;
using Org.BouncyCastle.Crypto.Signers;
using System.Linq;

namespace GrpcServer.Services
{
    public class ChatService : Chat.ChatBase
    {
        private static Dictionary<int, ServerCallContext> Subscribers = new();
        private readonly ILogger<ChatService> _logger;
        private readonly TalkzContext _DBContext;
        private static Msg? message;
        public ChatService(ILogger<ChatService> logger, TalkzContext DBContext)
        {
            _logger = logger;
            //_DBContext = DBContext;
        }

        public override async Task Subscribe(Request request, IServerStreamWriter<Response> responseStream, ServerCallContext context)
        {
            //KeyValuePair<int, ServerCallContext> foundPair = Subscribers.FirstOrDefault(x => x.Value == context);
            ServerCallContext isIdHere = Subscribers.FirstOrDefault(x => x.Key == request.Id).Value;
            if (isIdHere is not null)
            {
                _logger.LogInformation($"Subscribe Anfrage von {request.Id} abgelehnt, da schon vorhanden.");
                return;
            }
            _logger.LogInformation($"Subscribe Anfrage erhalten von: {request.Id}");
            Subscribers.Add(request.Id, context);
            await responseStream.WriteAsync(new Response { Data = $"Connected to Server!" });
            while (!context.CancellationToken.IsCancellationRequested)
            {
                if (message == null) continue;
                if (message.ToId == request.Id)
                {
                    await responseStream.WriteAsync(new Response { Data = message.Text, FromId = message.FromId });
                    message = null;
                }
                await Task.Delay(1);
            }
        }

        public override Task<Empty> SendMessage(Msg request, ServerCallContext context)
        {
            _logger.LogInformation($"Nachricht von {request.FromId} erhalten für: {request.ToId}\n Inhalt: {request.Text}");
            message = request;
            return Task.FromResult(new Empty());
        }
    }
}
