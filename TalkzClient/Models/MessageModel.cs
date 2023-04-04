using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TalkzClient.Models
{
    public class MessageModel
    {
        public string? Username { get; set; }
        public int FromId { get; set; }
        public DateTime Time { get; set; }
        public string? Message { get; set; }

    }
}
