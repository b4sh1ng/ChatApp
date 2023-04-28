using System;
using System.Collections.Generic;

namespace GrpcServer.Entities;

public partial class Message
{
    public long MessageTimestamp { get; set; }

    public int FromId { get; set; }

    public int ChatId { get; set; }

    public string Message1 { get; set; } = null!;

    public bool? IsEdited { get; set; }

    public bool? IsRead { get; set; }

    public int MessageId { get; set; }
}
