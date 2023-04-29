using ChatClient.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace ChatClient.ViewModels.FriendsTabViewModels
{
    partial class OnlineFriendsView : BaseView
    {
        public ObservableCollection<FriendModel>? FriendList { get; set; }
        public static ICollectionView FriendListCollection
        {
            get { return SharedData.FriendListCollection!; }
        }

        public OnlineFriendsView()
        {
            try
            {
                FriendListCollection.Filter = OnlineFriendsFilter;
                Application.Current.Dispatcher.Invoke((() => { FriendListCollection.Refresh(); }));
            }
            catch (Exception ex)
            {
               // MessageBox.Show(ex.Message);
            }
        }

        private bool OnlineFriendsFilter(object obj)
        {
            if (obj is FriendModel friend)
            {
                if (friend.IsFriend && (friend.CurrentStatus is SolidColorBrush solidColorBrush) && solidColorBrush.Color != Colors.Gray)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
