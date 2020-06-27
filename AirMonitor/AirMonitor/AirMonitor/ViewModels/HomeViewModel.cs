using System;
using AirMonitor.Views;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Essentials;
using System.Net;
using System.Net.Http;
using System.Collections.Generic;
using AirMonitor.Models;
using System.Linq;
using Newtonsoft.Json;
using System.Web;
using System.Globalization;

namespace AirMonitor.ViewModels
{
    public class HomeViewModel : BaseViewModel
    {
        Xamarin.Forms.INavigation navigation;

        private ICommand _goToDetailsCommand;
        public ICommand GoToDetailsCommand => _goToDetailsCommand ?? (_goToDetailsCommand = new Command(OnGoToDetails));

        private List<MeasurementData> _items;
        public List<MeasurementData> Items {
            get => _items;
            set => SetProperty(ref _items, value);
        }


        public HomeViewModel(Xamarin.Forms.INavigation navigation)
        {
            this.navigation = navigation;
            Initialize();
        }

        private async Task Initialize()
        {
            var location = await GetLocation();
            var installations = await GetInstallations(location, maxResults: 3);
            var data = new List<MeasurementData>();

            foreach (var item in installations) {
                var measurement = await GetMeasurement(item.Id);
                data.Add(new MeasurementData
                {
                    Installation = item,
                    MeasurementItem = measurement.Current
                });
            }
            Items = data;
        }

        private void OnGoToDetails()
        {
            navigation.PushAsync(new DetailsPage());
        }

        private async Task<Measurement> GetMeasurement(int id)
        {
            var query = GetQuery(new Dictionary<string, object>
            {
                { "installationId", id }
            });
            var url = GetAirlyApiUrl(App.AirlyApiMeasurementUrl, query);

            var response = await GetHttpResponseAsync<Measurement>(url);
            return response;
        }

        private static HttpClient GetHttpClient()
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri(App.AirlyApiUrl);

            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip");
            client.DefaultRequestHeaders.Add("apikey", App.AirlyApiKey);
            return client;
        }

        private string GetAirlyApiUrl(string path, string query)
        {
            var builder = new UriBuilder(App.AirlyApiUrl);
            builder.Path += path;
            builder.Query = query;
            builder.Port = -1;

            return builder.ToString();
        }

        private string GetQuery(Dictionary<string, object> args)
        {
            if (args == null) return null;

            var query = HttpUtility.ParseQueryString(string.Empty);

            foreach (var arg in args)
            {
                if (arg.Value is double number)
                {
                    query[arg.Key] = number.ToString(CultureInfo.InvariantCulture);

                }
                else
                {
                    query[arg.Key] = arg.Value?.ToString();
                }
            }

            return query.ToString();
        }

        private async Task<T> GetHttpResponseAsync<T>(string url)
        {
            try
            {
                var client = GetHttpClient();
                var response = await client.GetAsync(url);

                if (response.Headers.TryGetValues("X-RateLimit-Limit-day", out var dayLimit) &&
                    response.Headers.TryGetValues("X-RateLimit-Remaining-day", out var dayLimitRemaining)) {
                    System.Diagnostics.Debug.WriteLine($"Day limit: {dayLimit?.FirstOrDefault()}, remaining: {dayLimitRemaining?.FirstOrDefault()}");
                }

                switch ((int)response.StatusCode)
                {
                    case 200:
                        var content = await response.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<T>(content);
                        return result;
                    case 429:
                        System.Diagnostics.Debug.WriteLine("Too many request");
                        break;
                    default:
                        var errorContent = await response.Content.ReadAsStringAsync();
                        System.Diagnostics.Debug.WriteLine($"Response error: {errorContent}");
                        return default;
                }
            }
            catch (JsonReaderException ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }
            catch (WebException ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }

            return default;
        }

        private async Task<IEnumerable<Installation>> GetInstallations(Location location, double maxDistanceInKm = 3, int maxResults = -1)
        {
            if (location == null)
            {
                System.Diagnostics.Debug.WriteLine("No location data.");
                return null;
            }

            var query = GetQuery(new Dictionary<string, object>
            {
                { "lat", location.Latitude },
                { "lng", location.Longitude },
                { "maxDistanceKm", maxDistanceInKm },
                { "maxResults", maxResults },
            });
            var url = GetAirlyApiUrl(App.AirlyApiInstallationUrl, (string)query);

            var response = await GetHttpResponseAsync<IEnumerable<Installation>>((string)url);
            return response;
        }

        private async Task<Location> GetLocation()
        {
            Location location = await Geolocation.GetLastKnownLocationAsync();
            return location;
        }
    }
}
