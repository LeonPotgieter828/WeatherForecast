using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace WeatherForecast.ViewModels
{
    public class DailyViewModel
    {
        [JsonPropertyName("time")]
        public List<DateOnly> Date { get; set; }

        [JsonPropertyName("temperature_2m_max")]
        public List<double> MaxTempareture { get; set; }

        [JsonPropertyName("temperature_2m_min")]
        public List<double> MinTempareture { get; set; }

        [JsonPropertyName("wind_speed_10m_max")]
        public List<double> WindSpeed { get; set; }

        [JsonPropertyName("rain_sum")]
        public List<double> RainSum { get; set; }
    }
}
