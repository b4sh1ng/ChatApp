using ChatClient.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace ChatClient.ViewModels.FriendsTabViewModels
{
    partial class RequestsFriendsView : BaseView
    {
        public ObservableCollection<FriendModel>? FriendList { get; set; }
        public static ICollectionView FriendListCollection
        {
            get { return SharedData.FriendListCollection!; }
        }
        public RequestsFriendsView()
        {
            try
            {
                FriendListCollection.Filter = RequestingFriendsFilter;
                App.Current.Dispatcher.Invoke((() => { FriendListCollection.Refresh(); }));
            }
            catch (Exception ex)
            {
               // MessageBox.Show(ex.Message);
            }
        }
        private bool RequestingFriendsFilter(object obj)
        {
            if (obj is FriendModel friend)
            {
                if (!friend.IsFriend)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
