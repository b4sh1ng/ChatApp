using ChatClient.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatClient.ViewModels
{
    public partial class ChatView : BaseView
    {
        [ObservableProperty]
        private string? name;

        public ChatView()
        {

        }
        public ChatView(ChatModel? chatModel)
        {
            Name = chatModel.ChatName;
        }
    }
}
