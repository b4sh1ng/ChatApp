using ChatClient.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatClient.ViewModels.FriendsTabViewModels
{
    class AllFriendsView : BaseView
    {
        public ObservableCollection<FriendModel>? FriendList { get; set; }
        public string FriendCount { get; set; }

        public AllFriendsView(){ }
        public AllFriendsView(ObservableCollection<FriendModel> friendList)
        {
            FriendList = new ObservableCollection<FriendModel>(friendList.Where(x => x.IsFriend == true).ToList());
            FriendCount = $"All Friends ~ {FriendList.Count}";
        }
    }
}
