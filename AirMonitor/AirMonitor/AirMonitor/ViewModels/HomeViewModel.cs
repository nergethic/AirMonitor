using AirMonitor.Views;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Essentials;

namespace AirMonitor.ViewModels
{
    public class HomeViewModel
    {
        Xamarin.Forms.INavigation navigation;

        public HomeViewModel(Xamarin.Forms.INavigation navigation)
        {
            this.navigation = navigation;
        }

        private async Task Initialize()
        {
            var location = await GetLocation();
        }

        private ICommand _goToDetailsCommand;
        public ICommand GoToDetailsCommand => _goToDetailsCommand ?? (_goToDetailsCommand = new Command(OnGoToDetails));

        private void OnGoToDetails()
        {
            navigation.PushAsync(new DetailsPage());
        }

        private async Task<Location> GetLocation()
        {
            Location location = await Geolocation.GetLastKnownLocationAsync();
            return location;
        }
    }
}
