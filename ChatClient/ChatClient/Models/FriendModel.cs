using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatClient.Models
{
    public class FriendModel
    {
        public int FriendId { get; set; }
        public string? Username { get; set; }
        public int UsernameId { get; set; }
        public string? ImageSource { get; set; }
        public bool IsFriend { get; set; }
        public int CurrentStatus { get; set; }
    }
}
