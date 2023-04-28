using System;
using System.Collections.Generic;

namespace GrpcServer.Entities;

public partial class Chat
{
    public int UserId { get; set; }

    public int ChatId { get; set; }

    public bool IsListed { get; set; }
}
