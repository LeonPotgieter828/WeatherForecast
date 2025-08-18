using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace WeatherForecast.ViewModels
{
    public class LocationViewModel
    {
        [JsonPropertyName("longitude")]
        public double Longitude { get; set; }

        [JsonPropertyName("latitude")]
        public double Latitude { get; set; }

        [JsonPropertyName("admin1")]
        public string Region { get; set; }

        [JsonPropertyName("name")]
        public string City { get; set; }
    }
}
