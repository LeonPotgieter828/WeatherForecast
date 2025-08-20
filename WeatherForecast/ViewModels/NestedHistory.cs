using System.Text.Json.Serialization;

namespace WeatherForecast.ViewModels
{
    public class NestedHistory
    {
        [JsonPropertyName("daily")]
        public HistoryViewModel History { get; set; }
    }
}
