using System.ComponentModel.DataAnnotations;

namespace WeatherForecast.Models
{
    public class Location
    {
        [Key]
        public int LocationID { get; set; }
        [Required]
        public double Longitude { get; set; }
        [Required]
        public double Latitude { get; set; }
        [Required]
        public string Region { get; set; }
        [Required]
        public string City { get; set; }
    }
}
