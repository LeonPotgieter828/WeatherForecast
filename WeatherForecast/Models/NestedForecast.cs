using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace WeatherForecast.Models
{
    public class NestedForecast
    {
        [JsonPropertyName("current")]
        public Current CrForecast {  get; set; }

        [JsonPropertyName("hourly")]
        public Hourly HrForecas { get; set; }

        [JsonPropertyName("daily")]
        public Daily DlForecast { get; set; }
    }
}
