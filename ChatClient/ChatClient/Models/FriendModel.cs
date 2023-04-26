using ChatClient.Enums;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace ChatClient.Models
{
    public partial class FriendModel : ObservableObject
    {
        public int FriendId { get; set; }
        public string? Username { get; set; }
        public int UsernameId { get; set; }
        public string? ImageSource { get; set; }
        [ObservableProperty]
        private bool isFriend;
        [ObservableProperty]
        private SolidColorBrush currentStatus = StatusEnumHandler.GetStatusColor((State)0);
    }
}
