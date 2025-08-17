using Microsoft.EntityFrameworkCore;

namespace WeatherForecast.Models
{
    public class ForecastDbContext : DbContext
    {
        public ForecastDbContext(DbContextOptions options) : base(options) { }

        DbSet<Current> Current {  get; set; }
        DbSet<Hourly> Hourly { get; set; }
        DbSet<Daily> Daily { get; set; }
        DbSet<Location> Location { get; set; }
        DbSet<History> History { get; set; }
    }
}
