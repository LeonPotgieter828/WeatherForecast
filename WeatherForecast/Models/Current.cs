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
        [Required]
        public double Temperature { get; set; }
        [Required]
        public double WindSpeed { get; set; }
        [Required]
        public string WindDirection { get; set; }
        [Required]
        public string WeatherCode { get; set; }
        [Required]
        public bool IsDayTime { get; set; }
    }
}
