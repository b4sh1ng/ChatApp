using ChatClient.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

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
