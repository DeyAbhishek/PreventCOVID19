using Hackathon2020.Models;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Collections;

namespace WebAPITest
{
    class Program
    {
        private static string requestURI = "";
        static async Task Main(string[] args)
        {
            UserLocation userLocation1 = new UserLocation();
            userLocation1.UserId = Guid.NewGuid().ToString();//"123";
            userLocation1.dateTime = DateTime.Now;
            userLocation1.Location = new Microsoft.Azure.Cosmos.Spatial.Point(-122.131, 47.6518);

            await ClientPostUserLocationRequestAsync(userLocation1);

            UserLocation userLocation2 = new UserLocation();
            userLocation2.UserId = Guid.NewGuid().ToString();//"124";
            userLocation2.dateTime = DateTime.Now;
            userLocation2.Location = new Microsoft.Azure.Cosmos.Spatial.Point(-121.001, 45.6518);

            await ClientPostUserLocationRequestAsync(userLocation2);

            UserLocation userLocation = new UserLocation();
            userLocation.UserId = "1";
            userLocation.Location = new Microsoft.Azure.Cosmos.Spatial.Point(-122.130680, 47.651490);
            await FindAllNearbyUsersAsync(userLocation, 3000.0);
        }

        static async Task<HttpResponseMessage> FindAllNearbyUsersAsync(UserLocation userLocation, double radiusInMeters)
        {
            HttpClient client = new HttpClient
            {
                BaseAddress = new Uri(requestURI)
            };
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            //ArrayList paramList = new ArrayList();
            //paramList.Add(userLocation);
            //paramList.Add(radiusInMeters);

            HttpResponseMessage response = new HttpResponseMessage();
            try
            {
                 response = await client.PostAsJsonAsync($"api/location/GetUsersInRadius", userLocation);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message + "\r\n" + ex.StackTrace);
            }
            return response;

        }
        static async Task<HttpResponseMessage> ClientPostUserLocationRequestAsync(UserLocation userLocation)
        {
            HttpClient client = new HttpClient
            {
                BaseAddress = new Uri(requestURI)
            };
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await client.PostAsJsonAsync($"api/location", userLocation);
            return response;
        }
    }
}
