using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace WeatherForecast.ViewModels
{
    public class HourlyViewModel
    {
        [JsonPropertyName("temperature_2m")]
        public List<double> Temp { get; set; }

        [JsonPropertyName("time")]
        public List<DateTime> Time { get; set; }

        [JsonPropertyName("rain")]
        public List<double> Rain { get; set; }
    }
}
