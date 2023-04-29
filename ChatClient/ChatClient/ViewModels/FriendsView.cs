using ChatClient.Models;
using ChatClient.ViewModels.FriendsTabViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace ChatClient.ViewModels
{
    public partial class FriendsView : BaseView
    {
        private ObservableCollection<FriendModel> FriendsList;
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
        public FriendsView()
        {
            SelectedFriendsView = new OnlineFriendsView();
            Selected = "online";
        }
        private BaseView ViewSelector(string? parameter) => parameter switch
        {
            "all" => SelectedFriendsView = new AllFriendsView(),
            "add" => SelectedFriendsView = new AddFriendsView(),
            "online" => SelectedFriendsView = new OnlineFriendsView(),
            "requests" => SelectedFriendsView = new RequestsFriendsView(),
            _ => SelectedFriendsView = new OnlineFriendsView()
        };
    }
}
