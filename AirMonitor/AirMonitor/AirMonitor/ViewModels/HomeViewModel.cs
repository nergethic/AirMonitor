using AirMonitor.Views;
using System.Windows.Input;
using Xamarin.Forms;

namespace AirMonitor.ViewModels
{
    public class HomeViewModel
    {
        Xamarin.Forms.INavigation navigation;

        public HomeViewModel(Xamarin.Forms.INavigation navigation)
        {
            this.navigation = navigation;
        }

        private ICommand _goToDetailsCommand;
        public ICommand GoToDetailsCommand => _goToDetailsCommand ?? (_goToDetailsCommand = new Command(OnGoToDetails));

        private void OnGoToDetails()
        {
            navigation.PushAsync(new DetailsPage());
        }
    }
}
