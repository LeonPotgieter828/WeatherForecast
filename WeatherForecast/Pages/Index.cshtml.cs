using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using WeatherForecast.Models;

namespace WeatherForecast.Pages
{
    public class IndexModel : PageModel
    {
        string url = "https://api.open-meteo.com/v1/forecast?latitude=52.52&longitude=13.41&daily=temperature_2m_max,temperature_2m_min,wind_speed_10m_max,rain_sum&hourly=temperature_2m,rain&current=temperature_2m,wind_speed_10m,wind_direction_10m,weather_code,is_day&timezone=auto";
        private readonly ILogger<IndexModel> _logger;
        public Current currentForecast { get; set; }
        public NestedForecast nested { get; set; }

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public async Task OnGet()
        {
            currentForecast = await CurrentWeather();
            nested = new NestedForecast();
        }

        public async Task<Current> CurrentWeather()
        {
            using HttpClient client = new HttpClient();
            var getResponse = await client.GetAsync(url);
            if (getResponse.IsSuccessStatusCode)
            {
                var json = await getResponse.Content.ReadAsStringAsync();
                var currentWeather = JsonSerializer.Deserialize<NestedForecast>(json);
                return currentWeather.CrForecast;
            }
            return null;
        } 
    }
}
