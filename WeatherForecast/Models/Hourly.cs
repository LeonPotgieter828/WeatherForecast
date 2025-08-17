using System.ComponentModel.DataAnnotations;

namespace WeatherForecast.Models
{
    public class Hourly
    {
        [Key]
        public int HourlyID { get; set; }
        [Required]
        public double Tempareture { get; set; }
        [Required]
        public TimeOnly ForecastTime { get; set; }
        [Required]
        public double RainSum { get; set; }

    }
}
