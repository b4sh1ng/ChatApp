using ChatClient.Enums;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;

namespace ChatClient.Models
{
    public partial class ChatModel : ObservableObject
    {
        public string? ChatName { get; set; }
        public int ChatId { get; set; }
        public string? ImageSource { get; set; }
        public ObservableCollection<MessageModel>? Messages { get; set; }
        [ObservableProperty]
        private SolidColorBrush currentStatus = StatusEnumHandler.GetStatusColor((State)0);
        [ObservableProperty]
        private bool isChatListed;
        [ObservableProperty]
        private DateTime? latestMessageTime;
        // public bool IsGroupChat { get; set; }

        public ChatModel()
        {
            Messages = new();
            Messages.CollectionChanged += (sender, args) =>
            {
                if (args.NewItems != null)
                {
                    foreach (var newItem in args.NewItems.OfType<MessageModel>())
                    {
                        if (newItem.Time >= LatestMessageTime.GetValueOrDefault())
                        {
                            LatestMessageTime = newItem.Time;
                        }
                    }
                }
            };
        }
    }
}