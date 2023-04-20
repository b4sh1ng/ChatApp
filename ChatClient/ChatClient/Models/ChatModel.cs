using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatClient.Models
{
    public partial class ChatModel : ObservableObject
    {
        public string? ChatName { get; set; }
        public int ChatId { get; set; }
        public string? ImageSource { get; set; }
        public ObservableCollection<MessageModel>? Messages { get; set; }
        // public byte CurrentStatus { get; set; }
        [ObservableProperty]
        private bool isChatListed;
        // public bool IsGroupChat { get; set; }
    }
}
