using System;

namespace ChatClient.Models
{
    public class MessageModel
    {
        public string? Username { get; set; }
        public string? ImageSource { get; set; }
        public int FromId { get; set; }
        public string? Message { get; set; }
        public DateTime Time { get; set; }
        public bool IsEdited { get; set; }
        public bool IsRead { get; set; }
    }
}
