using ChatClient.Enums;
using ChatClient.Models;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

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
            try
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    FriendList = new ObservableCollection<FriendModel>(friendList
                    .Where(x => (x.CurrentStatus is SolidColorBrush solidColorBrush) && solidColorBrush.Color != Colors.Gray)
                    .ToList());
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            FriendCount = $"Online ~ {FriendList?.Count}";
        }
    }
}
