using Hackathon2020.Models;
using Hackathon2020.Utils;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using System.Web.Http;

namespace Hackathon2020.Controllers
{
    [RoutePrefix("api/location")]
    public class LocationController : ApiController
    {
        private string databaseId = ConfigurationManager.AppSettings["DatabaseId"];
        private string containerId = ConfigurationManager.AppSettings["ContainerId"];
        private string endpoint = ConfigurationManager.AppSettings["CosmosDbConnectionString"];
        private double searchRadius = Convert.ToDouble(ConfigurationManager.AppSettings["SearchRadius"]);

        
        [System.Web.Http.HttpPost]
        public async Task<bool> Post([FromBody] UserLocation userLocation)
        {
            try
            {
                // construct cosmosdb client and call upsert method
                CosmosDbClient cosmosDbClient = new CosmosDbClient(endpoint, "");
                cosmosDbClient.GetContainer(databaseId, containerId);

                bool result = await cosmosDbClient.UpsertUserLocation(userLocation);
                return result;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message + "\r\n" + ex.StackTrace);
            }
            return false;
        }


        [Route("GetUsersInRadius")]
        [System.Web.Http.HttpPost]
        public async System.Threading.Tasks.Task<List<UserLocation>> GetUsersInRadius(UserLocation userLocation)
        {
            // construct cosmosdb client and call get nearby users method
            CosmosDbClient cosmosDbClient = new CosmosDbClient(endpoint, "");
            cosmosDbClient.GetContainer(databaseId, containerId);

            //UserLocation userLocation = new UserLocation();
            //userLocation.UserId = "123";
            //userLocation.Location = new Microsoft.Azure.Cosmos.Spatial.Point(-122.131, 47.6518);
            //UserLocation userLocation = (UserLocation)Newtonsoft.Json.JsonConvert.DeserializeObject(paramList[0].ToString());
            //double distanceInMeters = (double)Newtonsoft.Json.JsonConvert.DeserializeObject(paramList[1].ToString());


            var result = await cosmosDbClient.FindAllNearbyUsers(userLocation, searchRadius); //can be configurable

            //if(result.Count > 0)
            //{
            //    List<UserLocation> userLocations = new List<UserLocation>();
            //    foreach(var userlocation in result)
            //    {
            //        UserLocation userLocation1 = new UserLocation();
            //        userLocation1.UserId = userlocation.UserId;
            //        userLocation1.Location = new Microsoft.Azure.Cosmos.Spatial.Point(userlocation.Location.Position.Longitude, userlocation.Location.Position.Latitude);
            //        userLocation1.dateTime = userlocation.dateTime;
            //        userLocations.Add(userLocation1);
            //    }
            //    return userLocations;
            //}
            return result;
        }
    }
}