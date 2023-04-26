namespace GrpcServer.Entities;

public partial class Usercredential
{
    public int UserId { get; set; }

    public string Username { get; set; } = null!;

    public int UsernameId { get; set; }

    public string Password { get; set; } = null!;

    public string? ProfileImgB64 { get; set; }

    public int CurrentStatus { get; set; }

    public int LastStatus { get; set; }

    public string Email { get; set; } = null!;

}
