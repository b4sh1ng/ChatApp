using ChatClient.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ChatClient.ViewModels
{
    public partial class ChatView : BaseView
    {
        [ObservableProperty]
        private string? chatName;
        public ObservableCollection<MessageModel>? Messages { get; set; }

        public ChatView()
        {
            
        }
        public ChatView(ChatModel? chatModel)
        {
            if (chatModel is not null)
            {
                Messages = new ObservableCollection<MessageModel>();
                ChatName = "@" + chatModel!.ChatName;
                Messages = chatModel.Messages;
            }

        }
    }
}
