using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace WeatherForecast.ViewModels
{
    public class CurrentViewModel
    {
        [JsonPropertyName("temperature_2m")]
        public double Tempareture { get; set; }
        [JsonPropertyName("time")]
        public DateTime Time { get; set; }

        [JsonPropertyName("wind_speed_10m")]
        public double WindSpeed { get; set; }

        [JsonPropertyName("wind_direction_10m")]
        public double WindDirection { get; set; }

        [JsonPropertyName("weather_code")]
        public int WeatherCode { get; set; }

        [JsonPropertyName("is_day")]
        public int IsDayTime { get; set; }

        public string DayOrNight { get; set; }
        public string WeatherCodeString { get; set; }
        public string WeatherImage { get; set; }
    }
}
