using Azure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Globalization;
using System.Text.Json;
using WeatherForecast.Models;
using WeatherForecast.ViewModels;

namespace WeatherForecast.Pages
{
    public class IndexModel : PageModel
    {
        string url = "https://api.open-meteo.com/v1/forecast?latitude=52.52&longitude=13.41&daily=temperature_2m_max,temperature_2m_min,wind_speed_10m_max,rain_sum&hourly=temperature_2m,rain&current=temperature_2m,wind_speed_10m,wind_direction_10m,weather_code,is_day&timezone=auto";
        string geoUrl = "https://nominatim.openstreetmap.org/reverse?";
        private readonly ILogger<IndexModel> _logger;
        private static readonly HttpClient _httpClient = new HttpClient();
        public NestedForecast nested;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public async Task OnGet()
        {
            nested = new NestedForecast
            {
                CrForecast = await CurrentWeather(),
                HrForecast = await HourlyWeather(),
                DlForecast = await DailyWeather(),
                Location = await GetLocationDetails()
            };
        }

        public async Task<HttpResponseMessage?> GetResponse()
        {
            var response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                return response;
            }
            return null;
        }

        public async Task<CurrentViewModel?> CurrentWeather()
        {
            var getResponse = await _httpClient.GetAsync(url);
            if (getResponse.IsSuccessStatusCode)
            {
                var json = await getResponse.Content.ReadAsStringAsync();
                var currentWeather = JsonSerializer.Deserialize<NestedForecast>(json);
                return currentWeather.CrForecast;
            }
            return null;
        } 

        public async Task<HourlyViewModel> HourlyWeather()
        {
            var getResponse = await _httpClient.GetAsync(url);
            if (getResponse.IsSuccessStatusCode)
            {
                var json = await getResponse.Content.ReadAsStringAsync();
                var hourlyWeather = JsonSerializer.Deserialize<NestedForecast>(json); 
                return Hours(hourlyWeather);
            }
            return null;
        }

        public async Task<DailyViewModel> DailyWeather()
        {
            var getResponse = await _httpClient.GetAsync(url);
            if (getResponse.IsSuccessStatusCode)
            {
                var json = await getResponse.Content.ReadAsStringAsync();
                var dailyWeather = JsonSerializer.Deserialize<NestedForecast>(json);
                return dailyWeather.DlForecast;
            }
            return null;
        }

        public async Task<LocationViewModel> GetLocationDetails()
        {
            var location = await Location();
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("WeatherForecast/1.0 (leonpot992@gmail.com)");
            var getResponse = await _httpClient.GetAsync($"https://nominatim.openstreetmap.org/reverse?lat={location.Latitude.ToString(CultureInfo.InvariantCulture)}&lon={location.Longitude.ToString(CultureInfo.InvariantCulture)}&format=json&addressdetails=1");
            if (getResponse.IsSuccessStatusCode)
            {
                var json = await getResponse.Content.ReadAsStringAsync();
                var locationDetails = JsonSerializer.Deserialize<NestedForecast>(json);
                return locationDetails.Location;
            }
            return null;
        }

        public async Task<LocationViewModel> Location()
        {
            var getResponse = await _httpClient.GetAsync(url);
            if (getResponse.IsSuccessStatusCode)
            {
                var json = await getResponse.Content.ReadAsStringAsync();
                var location = JsonSerializer.Deserialize<LocationViewModel>(json);
                return location;
            }
            return null;
        }

        public HourlyViewModel Hours(NestedForecast hourlyWeather)
        {
            var today = DateTime.Today;

            var filteredTimes = new List<DateTime>();
            var filteredTemps = new List<double>();
            var filteredRain = new List<double>();

            for (int i = 0; i < hourlyWeather.HrForecast.Time.Count; i++)
            {
                if (hourlyWeather.HrForecast.Time[i].Date == today)
                {
                    filteredTimes.Add(hourlyWeather.HrForecast.Time[i]);
                    filteredTemps.Add(hourlyWeather.HrForecast.Temp[i]);
                    filteredRain.Add(hourlyWeather.HrForecast.Rain[i]);
                }
            }

             hourlyWeather.HrForecast.Time = filteredTimes;
             hourlyWeather.HrForecast.Temp = filteredTemps;
             hourlyWeather.HrForecast.Rain = filteredRain;

            return hourlyWeather.HrForecast;
        }
    }
}
