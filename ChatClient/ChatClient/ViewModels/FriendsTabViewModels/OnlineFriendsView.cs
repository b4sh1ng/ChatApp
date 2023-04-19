using ChatClient.Models;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ChatClient.ViewModels.FriendsTabViewModels
{
    partial class OnlineFriendsView : BaseView
    {
        public ObservableCollection<FriendModel>? FriendList { get; set; }
        public string FriendCount { get; set; }

        public OnlineFriendsView()
        {

        }
        public OnlineFriendsView(ObservableCollection<FriendModel>? friendList)
        {
            FriendList = new ObservableCollection<FriendModel>(friendList.Where(x => x.CurrentStatus > 0).ToList());
            FriendCount = $"Online ~ {FriendList.Count}";
        }
    }
}
