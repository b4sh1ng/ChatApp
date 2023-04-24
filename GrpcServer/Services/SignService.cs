﻿using GrpcServer.Entities;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;

namespace GrpcLogin.Services;

public class SignService : Sign.SignBase
{
    private readonly ILogger<SignService> logger;
    private readonly TalkzContext dbcontext;

    public SignService(ILogger<SignService> logger, TalkzContext DBContext)
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
        var dbRequest = await dbcontext.Usercredentials.SingleOrDefaultAsync(x => x.Username == userEmail && x.Password == userPasswordHash);
        if (dbRequest is null)
        {
            return new SuccessMessage() { IsOk = false };
        }
        string sessionId = Guid.NewGuid().ToString();
        dbRequest.SessionId = sessionId;

        await dbcontext.SaveChangesAsync();
        return new SuccessMessage() { IsOk = true, Session = sessionId };        
    }
    public override async Task<IsValid> LoginWithSession(SessionLogin request, ServerCallContext context)
    {
        var userId = request.UserId;
        var sessionId = request.SessionId;
        var dbRequest = await dbcontext.Usercredentials.SingleOrDefaultAsync(x => x.UserId == userId && x.SessionId == sessionId);
        if (dbRequest is null)
        {
            return new IsValid() { IsOk = false };
        }
        
        return new IsValid() { IsOk = true };
    }
}