using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatClient.Models
{
    public class ChatModel
    {
        public string? ChatName { get; set; }
        public string? ImageSource { get; set; }
        public ObservableCollection<MessageModel>? Messages { get; set; }
        // public byte CurrentStatus { get; set; }
        // public bool IsChatListed { get; set; }
        // public bool IsGroupChat { get; set; }
    }
}
