using ChatClient.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace ChatClient.ViewModels.FriendsTabViewModels
{
    class AllFriendsView : BaseView
    {
        public ObservableCollection<FriendModel>? FriendList { get; set; }
        public static ICollectionView FriendListCollection
        {
            get { return SharedData.FriendListCollection!; }
        }

        public AllFriendsView()
        {
            try
            {
                FriendListCollection.Filter = AllFriendsFilter;
                //FriendListCollection.GroupDescriptions.Add(new PropertyGroupDescription(nameof(FriendModel.CurrentStatus)));
                App.Current.Dispatcher.Invoke(() => { FriendListCollection.Refresh(); });
            }
            catch (Exception ex)
            {
              //  MessageBox.Show(ex.Message);
            }
        }
        private bool AllFriendsFilter(object obj)
        {
            if (obj is FriendModel friend)
            {
                if (friend.IsFriend)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
