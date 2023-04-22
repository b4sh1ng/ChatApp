using ChatClient.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatClient.ViewModels.FriendsTabViewModels
{
    partial class RequestsFriendsView : BaseView
    {
        public ObservableCollection<FriendModel>? FriendList { get; set; }
        [ObservableProperty]
        private string friendCount;
        public RequestsFriendsView() { }
        public RequestsFriendsView(ObservableCollection<FriendModel> friendList)
        {
            FriendList = new ObservableCollection<FriendModel>(friendList.Where(x => x.IsFriend == false).ToList());
            FriendCount = $"Requests ~ {FriendList.Count}";
        }
    }
}
