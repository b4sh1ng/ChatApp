using ChatClient.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GrpcServer;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace ChatClient.ViewModels
{
    public partial class ChatView : BaseView
    {
        public Chat.ChatClient Client { get; set; }
        public ObservableCollection<MessageModel>? Messages { get; set; }
        #region Observable Propertys
        [ObservableProperty]
        private string? chatName;
        [ObservableProperty]
        private SolidColorBrush friendStatus;
        [ObservableProperty]
        private string? messageTo;
        [ObservableProperty]
        private string? imageSource;
        [ObservableProperty]
        private string? message;
        #endregion

        private int chatId;
        private int userId;
        private string sessionId;
        public ICommand SendCommand { get; set; }
        private async void Send()
        {
            if (string.IsNullOrWhiteSpace(Message))
                return;

            await Client.PostMessageAsync(new Msg
            {
                ChatId = chatId,
                FromId = userId,
                Text = Message,
                SessionId = sessionId,
            });
            Message = "";
        }

        public ChatView()
        {
            SendCommand = new RelayCommand(Send);
        }
        public ChatView(ChatModel? chatModel, Chat.ChatClient client, int userId, string sessionId)
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
                this.userId = userId;
                Client = client;
                FriendStatus = chatModel.CurrentStatus;
                this.sessionId = sessionId;
            }
        }
    }
}
