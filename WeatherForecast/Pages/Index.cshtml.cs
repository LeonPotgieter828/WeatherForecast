using Azure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.IdentityModel.Tokens;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text.Json;
using WeatherForecast.Models;
using WeatherForecast.Operations;
using WeatherForecast.ViewModels;

namespace WeatherForecast.Pages
{
    public class IndexModel : PageModel
    {
        [BindProperty]
        public string SearchName { get; set; }
        public double longitude { get; set; }
        public double latitude { get; set; }

        private readonly ILogger<IndexModel> _logger;

        private static readonly HttpClient _httpClient = new HttpClient();

        public AllWeatherNested nested { get; set; }
        public DbOperations _db { get; set; }
        public DbFallback _fallback { get; set; }
        public Task<bool> ApiSuccess { get; set; }

        public ForecastDbContext _forecast;

        public IndexModel(ILogger<IndexModel> logger, ForecastDbContext forecast)
        {
            _logger = logger;
            _forecast = forecast;
            _db = new DbOperations();
            _fallback = new DbFallback(_forecast);
        }

        public async Task OnGet()
        {
            nested = await BuildForecastAsync(SearchName);
            ApiSuccess = ApiResponse();
            _db.StoreAndUpdate(_forecast, nested, ApiSuccess);
        }

        public async Task<IActionResult> OnPost()
        {      
            nested = await BuildForecastAsync(SearchName);
            _db.StoreAndUpdate(_forecast, nested, ApiSuccess);
            return Page();
        }
        private async Task<AllWeatherNested> BuildForecastAsync(string searchName)
        {
            if (searchName.IsNullOrEmpty())
            {
                searchName = "Port Elizabeth";
                SearchName = searchName;
            }
            var nes = new AllWeatherNested
            {
                NestedF = new NestedForecast
                {
                    Location = await LocationSearch(searchName),
                    CrForecast = await CurrentWeather(searchName),
                    HrForecast = await HourlyWeather(searchName),
                    DlForecast = await DailyWeather(searchName),
                },

                NestedH = new NestedHistory
                {
                    History = await HistoryForecast(searchName)
                }
            };

            return nes;
        }

        public async Task<CurrentViewModel?> CurrentWeather(string name)
        {
            var getResponse = await _httpClient.GetAsync(ForecastURL());
            if (getResponse.IsSuccessStatusCode)
            {
                var json = await getResponse.Content.ReadAsStringAsync();
                var currentWeather = JsonSerializer.Deserialize<NestedForecast>(json);
                Console.WriteLine(json);
                return currentWeather.CrForecast;
            }
            return _fallback.CurrentFallback(name);
        } 

        public async Task<HourlyViewModel> HourlyWeather(string name)
        {
            var getResponse = await _httpClient.GetAsync(ForecastURL());
            if (getResponse.IsSuccessStatusCode)
            {
                var json = await getResponse.Content.ReadAsStringAsync();
                var hourlyWeather = JsonSerializer.Deserialize<NestedForecast>(json); 
                return Hours(hourlyWeather);
            }
            return _fallback.HourlyFallback(name);
        }

        public async Task<DailyViewModel> DailyWeather(string name)
        {
            var getResponse = await _httpClient.GetAsync(ForecastURL());
            if (getResponse.IsSuccessStatusCode)
            {
                var json = await getResponse.Content.ReadAsStringAsync();
                var dailyWeather = JsonSerializer.Deserialize<NestedForecast>(json);
                return dailyWeather.DlForecast;
            }
            return _fallback.DailyFallback(name);
        }

        public async Task<List<LocationViewModel>> LocationSearch(string name)
        {
            var getResponse = await _httpClient.GetAsync($"https://geocoding-api.open-meteo.com/v1/search?name={name}&count=1&language=en&format=json");
            if (getResponse.IsSuccessStatusCode)
            {
                var json = await getResponse.Content.ReadAsStringAsync();
                var location = JsonSerializer.Deserialize<NestedForecast>(json);
                var getLocation = location.Location.FirstOrDefault();
                longitude = getLocation.Longitude;
                latitude = getLocation.Latitude;
                return location.Location;
            }
            return _fallback.LocationFallback(name);
        }

        public async Task<HistoryViewModel> HistoryForecast(string name)
        {
            var getResponse = await _httpClient.GetAsync(HistoryURL());
            if (getResponse.IsSuccessStatusCode)
            {
                var json = await getResponse.Content.ReadAsStringAsync();
                var history = JsonSerializer.Deserialize<NestedHistory>(json);
                return history.History;
            }
            return _fallback.HistoryFallback(name);
        }

        public async Task<bool> ApiResponse()
        {
            var getResponse = await _httpClient.GetAsync(ForecastURL());
            if (getResponse.IsSuccessStatusCode)
            {
                return true;
            }
            return false;
        }

        public string ForecastURL()
        {
            string url = $"https://api.open-meteo.com/v1/forecast?latitude={latitude.ToString(CultureInfo.InvariantCulture)}&longitude={longitude.ToString(CultureInfo.InvariantCulture)}&daily=temperature_2m_max,temperature_2m_min,wind_speed_10m_max,rain_sum&hourly=temperature_2m,rain&current=temperature_2m,wind_speed_10m,wind_direction_10m,weather_code,is_day&timezone=auto";
            return url;
        }

        public string HistoryURL()
        {
            string url = $"https://api.open-meteo.com/v1/forecast?latitude={latitude.ToString(CultureInfo.InvariantCulture)}&longitude={longitude.ToString(CultureInfo.InvariantCulture)}&daily=weather_code,temperature_2m_max,rain_sum,wind_speed_10m_max,temperature_2m_min&current=weather_code&past_days=5&forecast_days=1";
            return url;
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
