using ChatClient.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatClient.ViewModels.FriendsTabViewModels
{
    class RequestsFriendsView : BaseView
    {
        public ObservableCollection<FriendModel>? FriendList { get; set; }

        public RequestsFriendsView()
        {
            FriendList = new();
            for (int i = 0; i < 1; i++)
            {
                FriendList.Add(new FriendModel()
                {
                    FriendId = 1,
                    Username = "Artem",
                    UsernameId = 1,
                    ImageSource = "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",
                    IsFriend = true,
                });
            }

        }
    }
}
