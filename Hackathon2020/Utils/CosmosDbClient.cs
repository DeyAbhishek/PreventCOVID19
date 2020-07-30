using Hackathon2020.Models;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;

namespace Hackathon2020.Utils
{
    public class CosmosDbClient
    {
        private CosmosClient cosmosClient;
        private Container container;


        public CosmosDbClient(string endpoint, string primaryKey)
        {
            try
            {
                cosmosClient = new CosmosClient(endpoint);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public CosmosDbClient GetContainer(string databaseId, string containerId)
        {
            container = cosmosClient.GetContainer(databaseId, containerId);
            return this;
        }

        public async Task<bool> UpsertUserLocation(UserLocation userLocation)
        {
            // if present then update or else create new
            try
            {
                // Read the item to see if it exists. Note ReadItemAsync will not throw an 
                // exception if an item does not exist. Instead, we check the StatusCode property 
                // of the response object. 
                ItemResponse<UserLocation> userLocationResponse = 
                    await container.ReadItemAsync<UserLocation>(
                        userLocation.UserId, new PartitionKey(userLocation.UserId));
                // replace json
                // replace the item with the updated content
                await container.ReplaceItemAsync<UserLocation>(userLocation, userLocation.UserId, new PartitionKey(userLocation.UserId));
                return true;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // Create an item in the container 
                await container.CreateItemAsync<UserLocation>(userLocation, new PartitionKey(userLocation.UserId));
                return true;
            }
            return false;
        }

        public async void DeleteUser(string userId)
        {
            container.DeleteItemAsync<UserLocation>(userId, new PartitionKey(userId));
        }

        public async Task<List<UserLocation>> FindAllNearbyUsers(UserLocation location, double radiusInMeters)
        {
            // https://docs.microsoft.com/en-us/azure/cosmos-db/sql-query-geospatial-query
            string sqlQueryText = @"SELECT * FROM UserLocation ul WHERE ST_DISTANCE(ul.location, { ""type"": ""Point"", ""coordinates"":[" + location.Location.Position.Longitude + " , " + location.Location.Position.Latitude + "]}) < " + radiusInMeters + "";
            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            List<UserLocation> result = new List<UserLocation>();
            using (FeedIterator<UserLocation> queryResultSetIterator = container.GetItemQueryIterator<UserLocation>(queryDefinition))
            {
                while (queryResultSetIterator.HasMoreResults)
                {
                    foreach (var userLocation in await queryResultSetIterator.ReadNextAsync())
                    {
                        result.Add(userLocation);
                    }
                }
            }
            return result;
        }
    }
}