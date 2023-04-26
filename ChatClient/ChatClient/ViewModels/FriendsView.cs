using ChatClient.Models;
using ChatClient.ViewModels.FriendsTabViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatClient.ViewModels
{
    public partial class FriendsView : BaseView
    {
        private ObservableCollection<FriendModel> FriendsList;
        public FriendsView()
        {
            SelectedFriendsView = new OnlineFriendsView();
            Selected = "online";
        }
        public FriendsView(ObservableCollection<FriendModel>? friendsList)
        {
            FriendsList = friendsList;
            SelectedFriendsView = new OnlineFriendsView(FriendsList);
            Selected = "online";
        }

        [ObservableProperty]
        private BaseView? selectedFriendsView;
        [ObservableProperty]
        private string? selected;
        [RelayCommand]
        private void ChangeFriendsView(string? parameter)
        {
            SelectedFriendsView = ViewSelector(parameter);
            Selected = parameter;
        }
        private BaseView ViewSelector(string? parameter) => parameter switch
        {
            "all" => SelectedFriendsView = new AllFriendsView(FriendsList),
            "add" => SelectedFriendsView = new AddFriendsView(),
            "online" => SelectedFriendsView = new OnlineFriendsView(FriendsList),
            "requests" => SelectedFriendsView = new RequestsFriendsView(FriendsList),
            _ => SelectedFriendsView = new OnlineFriendsView(FriendsList)
        };
    }
}
