using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatClient.Models
{
    public class MessageModel
    {
        public string? Username { get; set; }
        public int FromId { get; set; }
        public int MessageId { get; set; }
        public string? Message { get; set; }
        public DateTime Time { get; set; }
        public bool IsEdited { get; set; }
        public bool IsRead { get; set; }

    }
}
