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
        private readonly TalkzContext _DBContext;
        public GreeterService(ILogger<GreeterService> logger, TalkzContext DBContext)
        {
            _logger = logger;
            _DBContext = DBContext;
        }

        public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        {
            var test = _DBContext.Usercredentials.FirstOrDefault(s => s.UserId == 3)!;
            return Task.FromResult(new HelloReply
            {
                Message = $"Hello {test.Username}"
            });
        }
        //https://www.webnethelper.com/2021/06/grpc-services-using-net-5-using-entity.html


    }
}
