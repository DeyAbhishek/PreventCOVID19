using System;
using System.Collections.Generic;
using System.ComponentModel;
using Xamarin.Forms;
using Plugin.Geolocator.Abstractions;
using Plugin.Geolocator;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using System.Net.Http;
using Hackathon2020AndroidApp.Models;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Xamarin.Essentials;

namespace Hackathon2020AndroidApp
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    //[DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        private bool hasLocationPermision = false;
        private string requestURI = "";
        private string DeviceUniqueId { get; set; }
        public MainPage()
        {
            InitializeComponent();
           // DisplayUsersWithinSixFeetAsync(47.651490 , -122.130680);//(e.Position.Latitude, e.Position.Longitude);
           // ClientPostUserLocationRequestAsync(47.661001, -122.110680);
            
            //Get and Set DeviceId
            var id = Preferences.Get("my_id", string.Empty);
            if (string.IsNullOrWhiteSpace(id))
            {
                DeviceUniqueId = System.Guid.NewGuid().ToString();
                Preferences.Set("my_id", DeviceUniqueId);
            }
            GetPermission();
        }

        private async void GetPermission()
        {
            try
            {
                var permissionGrantStatus = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.LocationWhenInUse);
                if (permissionGrantStatus != PermissionStatus.Granted)
                {
                    if (await CrossPermissions.Current.ShouldShowRequestPermissionRationaleAsync(Permission.LocationWhenInUse))
                    {
                        await DisplayAlert("Location Permission Needed", "Location Permission Needed", "Ok");
                    }
                }
                var results = await CrossPermissions.Current.RequestPermissionsAsync(Permission.LocationWhenInUse);
                if (!results.ContainsKey(Permission.LocationWhenInUse))
                {
                    permissionGrantStatus = results[Permission.LocationWhenInUse];
                }

                if (permissionGrantStatus == PermissionStatus.Granted)
                {
                    hackathonAppMap.IsShowingUser = true;
                    hasLocationPermision = true;
                    GetLocation(); // show current in map
                }
                else
                {
                    await DisplayAlert("Permission Denied", "Location Permission Denied", "Ok");
                }

            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "Ok");
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (hasLocationPermision)
            {
                GetLocation();
                var locator = CrossGeolocator.Current;
                locator.PositionChanged += Locator_PositionChangedAsync;
                await locator.StartListeningAsync(TimeSpan.Zero, 1.8); // 6 ft = 1.8 mt
            }
        }
        protected override async void OnDisappearing()
        {
            base.OnDisappearing();
            await CrossGeolocator.Current.StopListeningAsync(); 
        }

        private async void GetLocation()
        {
            var locator = Plugin.Geolocator.CrossGeolocator.Current;
            var position = await locator.GetPositionAsync();
            var center = new Xamarin.Forms.Maps.Position(position.Latitude, position.Longitude);
            var span = new Xamarin.Forms.Maps.MapSpan(center, 1, 1);
            hackathonAppMap.MoveToRegion(span);
        }

        private async void Locator_PositionChangedAsync(object sender, PositionEventArgs e)
        {
            var center = new Xamarin.Forms.Maps.Position(e.Position.Latitude, e.Position.Longitude);
            var span = new Xamarin.Forms.Maps.MapSpan(center, 1, 1);
            hackathonAppMap.MoveToRegion(span);
            var userLocationList = await DisplayUsersWithinSixFeetAsync(e.Position.Latitude, e.Position.Longitude);
            if (userLocationList != null && userLocationList.Count > 0)
            {
                DrawUserLocationOnMap(userLocationList);
                // display social distancing alert
            }
            
        }
        private readonly object mapLock = new object();
        private void DrawUserLocationOnMap(List<UserLocation> userLocationList)
        {
            lock (mapLock)
            {
                foreach (var userlocation in userLocationList)
                {
                    var pin = new Xamarin.Forms.Maps.Pin()
                    {
                        Type = Xamarin.Forms.Maps.PinType.Place,
                        Label = "",
                        Position = new Xamarin.Forms.Maps.Position(userlocation.Location.Position.Latitude, userlocation.Location.Position.Longitude),
                    };
                    hackathonAppMap.Pins.Add(pin);
                    Console.WriteLine($"Latitude:{userlocation.Location.Position.Latitude} Longitude: {userlocation.Location.Position.Longitude}");
                }
            }
        }

        private async System.Threading.Tasks.Task<List<UserLocation>> DisplayUsersWithinSixFeetAsync(double latitude, double longitude)
        {
            // TODO
            // fetch data from cosmos db
            UserLocation userLocation = new UserLocation()
            {
                Location = new Microsoft.Azure.Cosmos.Spatial.Point(longitude, latitude),
                UserId = DeviceUniqueId,
            };

            using (HttpClient client = new HttpClient(new HttpClientHandler()))
            {
                client.BaseAddress = new Uri(requestURI);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                try
                {
                    //HttpResponseMessage response = await client.GetAsync($"api/values");
                    //var content = await response.Content.ReadAsStringAsync();
                    //var output = JsonConvert.DeserializeObject<string>(await response.Content.ReadAsStringAsync());
                    HttpResponseMessage response = await client.PostAsJsonAsync($"api/location/GetUsersInRadius", userLocation);
                    var content = await response.Content.ReadAsStringAsync();
                    // var output = JsonConvert.DeserializeObject<List<UserLocation>>(content1);
                    var settings = new JsonSerializerSettings
                    {
                        Converters = { new ArrayReferencePreservngConverter() },
                        PreserveReferencesHandling = PreserveReferencesHandling.All
                    };
                    var userLocationList  = JsonConvert.DeserializeObject<List<UserLocation>>(content, settings);
                    return userLocationList;
                    //loop through and plot on the graph..... Todo..
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message + "\r\n" + ex.StackTrace);
                }
                return null;
            }

    }

        private async System.Threading.Tasks.Task<bool> ClientPostUserLocationRequestAsync(double latitude, double longitude)
        {
            UserLocation userLocation = new UserLocation()
            {
                Location = new Microsoft.Azure.Cosmos.Spatial.Point(longitude, latitude),
                UserId = "3"
            };
            using (HttpClient client = new HttpClient(new HttpClientHandler()))
            {
                client.BaseAddress = new Uri(requestURI);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                try
                {
                    HttpResponseMessage response = await client.PostAsJsonAsync($"api/location", userLocation);
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message + "\r\n" + ex.StackTrace);
                }
                return false;
            }
        }
    }
}
