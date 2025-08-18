using System.Text.Json.Serialization;

namespace WeatherForecast.ViewModels
{
    public class HistoryViewModel
    {
        [JsonPropertyName("temperature_2m_max")]
        public List<double> Temp {  get; set; }

        [JsonPropertyName("wind_speed_10m_max")]
        public List<double> WindSpeed { get; set; }

        [JsonPropertyName("time")]
        public List<DateOnly> Recorded { get; set; }

        [JsonPropertyName("rain_sum")]
        public List<double> Rain { get; set; }
    }
}
