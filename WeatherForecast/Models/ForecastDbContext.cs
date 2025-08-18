using Microsoft.EntityFrameworkCore;

namespace WeatherForecast.Models
{
    public class ForecastDbContext : DbContext
    {
        public ForecastDbContext(DbContextOptions options) : base(options) { }

        public DbSet<Current> Current {  get; set; }
        public DbSet<Hourly> Hourly { get; set; }
        public DbSet<Daily> Daily { get; set; }
        public DbSet<Location> Location { get; set; }
        public DbSet<History> History { get; set; }
    }
}
