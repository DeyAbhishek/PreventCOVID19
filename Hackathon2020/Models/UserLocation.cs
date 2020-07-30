using System;
using Newtonsoft.Json;
using Microsoft.Azure.Cosmos.Spatial;

namespace Hackathon2020.Models
{
    public class UserLocation
    {
        [JsonProperty(PropertyName = "id")]
        public string UserId { get; set; }

        [JsonProperty(PropertyName = "location")]
        public Point Location { get; set; }

        [JsonProperty(PropertyName = "updated")]
        public DateTime dateTime { get; set; }
    }
}