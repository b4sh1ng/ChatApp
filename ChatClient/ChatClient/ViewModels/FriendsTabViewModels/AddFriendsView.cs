using ChatClient.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatClient.ViewModels.FriendsTabViewModels
{
    class AddFriendsView : BaseView
    {
        public ObservableCollection<FriendModel>? FriendList { get; set; }

        public AddFriendsView()
        {
            
        }
        public AddFriendsView(ObservableCollection<FriendModel>? friendList)
        {
            FriendList = friendList;
        }
    }
}
