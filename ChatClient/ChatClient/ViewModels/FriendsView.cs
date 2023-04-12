using ChatClient.ViewModels.FriendsTabViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatClient.ViewModels
{
    public partial class FriendsView : BaseView
    {
        public FriendsView()
        {
            SelectedFriendsView = new OnlineFriendsView();
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
            "all" => SelectedFriendsView = new AllFriendsView(),
            "add" => SelectedFriendsView = new AddFriendsView(),
            "online" => SelectedFriendsView = new OnlineFriendsView(),
            "requests" => SelectedFriendsView = new RequestsFriendsView(),
            _ => SelectedFriendsView = new OnlineFriendsView()
        };

    }
}
