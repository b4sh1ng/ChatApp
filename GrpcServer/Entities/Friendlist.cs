using System;
using System.Collections.Generic;

namespace GrpcServer.Entities;

public partial class Friendlist
{
    public int UserId1 { get; set; }

    public int UserId2 { get; set; }

    public bool? IsFriend { get; set; }    
}
