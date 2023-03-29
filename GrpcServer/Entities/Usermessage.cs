using System;
using System.Collections.Generic;

namespace GrpcServer.Entities;

public partial class Usermessage
{
    public int ChatId { get; set; }

    public string Message { get; set; } = null!;

    public long MessageTimestamp { get; set; }

    public bool? IsEdited { get; set; }

    public bool? IsRead { get; set; }
}
