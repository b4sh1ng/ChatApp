using GrpcLogin.Services;
using GrpcServer.Entities;
using GrpcServer.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
builder.Services.AddGrpc();
builder.Services.AddDbContext<ChatContext>(options =>
{
    options.UseMySQL(builder.Configuration.GetConnectionString("DefaultConnection")!);
});
builder.Services.AddControllers();
var app = builder.Build();
// Configure the HTTP request pipeline.
app.MapGrpcService<ChatService>();
app.MapGrpcService<SignService>();
app.Run();
