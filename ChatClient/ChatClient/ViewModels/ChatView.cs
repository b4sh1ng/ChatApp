using ChatClient.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Grpc.Core;
using Grpc.Net.Client;
using GrpcServer;
using System.Threading.Channels;

namespace ChatClient.ViewModels
{
    public partial class ChatView : BaseView
    {
        private int chatId;
        private int userId;
        public Chat.ChatClient Client { get; set; }
        [ObservableProperty]
        private string? chatName;

        [ObservableProperty]
        private string? messageTo;
        public ObservableCollection<MessageModel>? Messages { get; set; }
        [ObservableProperty]
        private string? imageSource;
        [ObservableProperty]
        private string? message;
        public ICommand SendCommand { get; set; }
        private async void Send()
        {
            await Client.PostMessageAsync(new Msg
            {
                ChatId = chatId,
                FromId = userId,
                Text = Message,
            });
            Message = "";
        }

        public ChatView()
        {
            SendCommand = new RelayCommand(Send);
        }
        public ChatView(ref ChatModel? chatModel, Chat.ChatClient client)
        {
            SendCommand = new RelayCommand(Send);
            if (chatModel is not null)
            {
                Messages = new ObservableCollection<MessageModel>();
                ChatName = "@" + chatModel!.ChatName;
                Messages = chatModel.Messages;
                ImageSource = chatModel.ImageSource;
                MessageTo = $"@{chatModel!.ChatName}";
                chatId = chatModel.ChatId;
                userId = 1;
                Client = client;
            }
        }
    }
}
