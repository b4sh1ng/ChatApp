using ChatClient.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatClient.ViewModels.FriendsTabViewModels
{
    partial class RequestsFriendsView : BaseView
    {
        public ObservableCollection<FriendModel>? FriendList { get; set; }
        public ICollectionView FriendListCollectionView { get; set; }
        [ObservableProperty]
        private string friendCount;
        public RequestsFriendsView() { }
        public RequestsFriendsView(ObservableCollection<FriendModel> friendList)
        {
            FriendList = new ObservableCollection<FriendModel>(friendList.Where(x => x.IsFriend == false).ToList());
            FriendCount = $"Requests ~ {FriendList.Count}";
        }
        public RequestsFriendsView(ICollectionView friendList)
        {
            FriendListCollectionView.Contains(friendList);
            FriendListCollectionView.Filter = RequestingFriendsFilter;
        }

        private bool RequestingFriendsFilter(object obj)
        {
            if(obj is FriendModel friend)
            {
                return friend.IsFriend.Equals(false);
            }
            return false;
        }
    }
}
