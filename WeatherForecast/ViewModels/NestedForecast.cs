using Newtonsoft.Json;
using System.Text.Json.Serialization;
using WeatherForecast.Models;

namespace WeatherForecast.ViewModels
{
    public class NestedForecast
    {
        [JsonPropertyName("current")]
        public CurrentViewModel CrForecast { get; set; }

        [JsonPropertyName("hourly")]
        public HourlyViewModel HrForecast { get; set; }

        [JsonPropertyName("daily")]
        public Daily DlForecast { get; set; }
    }
}
