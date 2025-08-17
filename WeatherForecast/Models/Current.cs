using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace WeatherForecast.Models
{
    public class Current
    {
        [Key]
        public int CurrentID { get; set; }
        [Required]
        public int LocationID { get; set; }

        [JsonPropertyName("temperature_2m")]
        [Required]
        public double Tempareture { get; set; }

        [JsonPropertyName("wind_speed_10m")]
        [Required]
        public double WindSpeed { get; set; }

        [JsonPropertyName("wind_direction_10m")]
        [Required]
        public string WindDirection { get; set; }

        [JsonPropertyName("weather_code")]
        [Required]
        public string WeatherCode { get; set; }

        [JsonPropertyName("is_day")]
        [Required]
        public bool IsDayTime { get; set; }
    }
}
