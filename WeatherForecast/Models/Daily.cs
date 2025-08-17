using System.ComponentModel.DataAnnotations;

namespace WeatherForecast.Models
{
    public class Daily
    {
        [Key]
        public int DailyID { get; set; }
        [Required]
        public int LocationID {  get; set; }
        [Required]
        public double MaxTempareture { get; set; }
        [Required]
        public double MinTempareture { get; set; }
        [Required]
        public double WindSpeed { get; set; }
        [Required]
        public double RainSum { get; set; }
    }
}
