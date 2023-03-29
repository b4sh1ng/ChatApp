using Grpc.Core;
using GrpcServer;
using GrpcServer.DTO;
using GrpcServer.Entities;
using System.Linq;

namespace GrpcServer.Services
{
    public class GreeterService : Greeter.GreeterBase
    {
        private readonly ILogger<GreeterService> _logger;
        public GreeterService(ILogger<GreeterService> logger, TalkzContext DBContext)
        {
            _logger = logger;
            _DBContext = DBContext;
        }

        private readonly TalkzContext _DBContext;

        public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        {
            ReplyDTO? test = _DBContext.Usercredentials.Select(s => new ReplyDTO
            {
                UserId = s.UserId
            }).FirstOrDefault();
            return Task.FromResult(new HelloReply
            {
                Message = $"Hello {request.Name} and {test}"
            });
        }
    }
}