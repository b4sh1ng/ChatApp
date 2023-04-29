using GrpcServer.Entities;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;

namespace GrpcLogin.Services;

public class SignService : Sign.SignBase
{
    private readonly ILogger<SignService> logger;
    private readonly ChatContext dbcontext;

    public SignService(ILogger<SignService> logger, ChatContext DBContext)
    {
        this.logger = logger;
        dbcontext = DBContext!;
    }
    public override Task<IsValid> Register(RegisterData request, ServerCallContext context)
    {
        return base.Register(request, context);
    }
    public override async Task<SuccessMessage> LoginWithUsername(LoginUser request, ServerCallContext context)
    {
        var userEmail = request.Email;
        var userPasswordHash = request.PasswordHash;
        var dbRequest = await dbcontext.Usercredentials.SingleOrDefaultAsync(x => x.Email == userEmail && x.Password == userPasswordHash);
        if (dbRequest is null)
        {
            return new SuccessMessage() { IsOk = false };
        }
        else
        {
            string sessionId = Guid.NewGuid().ToString();
            var createSession = dbcontext.Usersessions.Add(new Usersession
            {
                IsExpired = false,
                SessionId = sessionId,
                UserId = dbRequest.UserId
            });
            await dbcontext.SaveChangesAsync();

            return new SuccessMessage()
            {
                IsOk = true,
                Session = sessionId,
                UserId = dbRequest.UserId
            };
        }
    }
    public override async Task<IsValid> LoginWithSession(SessionLogin request, ServerCallContext context)
    {
        var userId = request.UserId;
        var sessionId = request.SessionId;
        var dbRequest = await dbcontext.Usersessions.SingleOrDefaultAsync(x => x.UserId == userId && x.SessionId == sessionId);
        if (dbRequest is null || dbRequest.IsExpired)
        {
            logger.LogInformation($"[{DateTime.Now:H:mm:ss:FFF}] Sessionlogin von User {request.UserId} nicht erfolgreich!");
            return new IsValid() { IsOk = false };
        }
        logger.LogInformation($"[{DateTime.Now:H:mm:ss:FFF}] Sessionlogin von User {request.UserId} erfolgreich!");
        return new IsValid() { IsOk = true };
    }
    public override async Task<Empty> Logout(SessionLogin request, ServerCallContext context)
    {
        var sessionCheckRequest = await dbcontext.Usersessions.SingleOrDefaultAsync(x => x.UserId == request.UserId && x.SessionId == request.SessionId);
        if (sessionCheckRequest == null) return new Empty();
        sessionCheckRequest.IsExpired = true;
        await dbcontext.SaveChangesAsync();
        return new Empty();
    }
}
