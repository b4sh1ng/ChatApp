using System;
using System.Collections.Generic;

namespace GrpcServer.Entities;

public partial class Usersession
{
    public int UserId { get; set; }

    public string SessionId { get; set; } = null!;

    public DateTime CreationDate { get; set; }

    public bool IsExpired { get; set; }
}
