using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GrpcServer.Entities;

public partial class Message
{
    public int ChatId { get; set; }

    public string Message1 { get; set; } = null!;
    [Key]
    public long MessageTimestamp { get; set; } 

    public bool? IsEdited { get; set; }

    public bool? IsRead { get; set; }
    [Key]
    public int FromId { get; set; }
}
