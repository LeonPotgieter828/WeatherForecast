using System.ComponentModel.DataAnnotations;

namespace WeatherForecast.Models
{
    public class History
    {
        [Key]
        public int HistoryID { get; set; }
        [Required]
        public int LocationID { get; set; }
        [Required]
        public double Tempareture { get; set; }
        [Required]
        public double WindSpeed { get; set; }
        [Required]
        public DateOnly RecordedAt { get; set; }    
        [Required]
        public double RainSum { get; set; } 
    }
}
