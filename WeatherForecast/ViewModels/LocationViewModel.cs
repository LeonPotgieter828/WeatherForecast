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

        [JsonPropertyName("state")]
        public string Region { get; set; }

        [JsonPropertyName("city")]
        public string City { get; set; }
    }
}
