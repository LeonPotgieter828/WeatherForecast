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
        public ForecastDbContext _forecast;

        public IndexModel(ILogger<IndexModel> logger, ForecastDbContext forecast)
        {
            _logger = logger;
            _forecast = forecast;
            _db = new DbOperations();
        }

        public async Task OnGet()
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue
            {
                NoCache = true,
                NoStore = true,
                MustRevalidate = true
            };
            nested = await BuildForecastAsync(SearchName);
            _db.StoreAndUpdate(_forecast, nested);
        }

        public async Task<IActionResult> OnPost()
        {
           
            nested = await BuildForecastAsync(SearchName);
            _db.StoreAndUpdate(_forecast, nested);
            return Page();
        }
        private async Task<AllWeatherNested> BuildForecastAsync(string searchName)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue
            {
                NoCache = true,
                NoStore = true,
                MustRevalidate = true
            };
            var nes = new AllWeatherNested
            {
                NestedF = new NestedForecast
                {
                    Location = await LocationSearch(searchName, client),
                    CrForecast = await CurrentWeather(client),
                    HrForecast = await HourlyWeather(client),
                    DlForecast = await DailyWeather(client),
                },

                NestedH = new NestedHistory
                {
                    History = await HistoryForecast(client)
                }
            };

            return nes;
        }

        public async Task<CurrentViewModel?> CurrentWeather(HttpClient client)
        {

            var getResponse = await client.GetAsync(ForecastURL());
            if (getResponse.IsSuccessStatusCode)
            {
                var json = await getResponse.Content.ReadAsStringAsync();
                var currentWeather = JsonSerializer.Deserialize<NestedForecast>(json);
                Console.WriteLine(json);
                return currentWeather.CrForecast;
            }
            return null;
        } 

        public async Task<HourlyViewModel> HourlyWeather(HttpClient client)
        {
            var getResponse = await client.GetAsync(ForecastURL());
            if (getResponse.IsSuccessStatusCode)
            {
                var json = await getResponse.Content.ReadAsStringAsync();
                var hourlyWeather = JsonSerializer.Deserialize<NestedForecast>(json); 
                return Hours(hourlyWeather);
            }
            return null;
        }

        public async Task<DailyViewModel> DailyWeather(HttpClient client)
        {
            var getResponse = await client.GetAsync(ForecastURL());
            if (getResponse.IsSuccessStatusCode)
            {
                var json = await getResponse.Content.ReadAsStringAsync();
                var dailyWeather = JsonSerializer.Deserialize<NestedForecast>(json);
                return dailyWeather.DlForecast;
            }
            return null;
        }

        public async Task<List<LocationViewModel>> LocationSearch(string name, HttpClient client)
        {

            if (name.IsNullOrEmpty())
            {
                name = "Port Elizabeth";
            }
            var getResponse = await client.GetAsync($"https://geocoding-api.open-meteo.com/v1/search?name={name}&count=1&language=en&format=json");
            if (getResponse.IsSuccessStatusCode)
            {
                var json = await getResponse.Content.ReadAsStringAsync();
                var location = JsonSerializer.Deserialize<NestedForecast>(json);
                var getLocation = location.Location.FirstOrDefault();
                longitude = getLocation.Longitude;
                latitude = getLocation.Latitude;
                return location.Location;
            }
            return null;
        }

        public async Task<HistoryViewModel> HistoryForecast(HttpClient client)
        {
            var getResponse = await client.GetAsync(HistoryURL());
            if (getResponse.IsSuccessStatusCode)
            {
                var json = await getResponse.Content.ReadAsStringAsync();
                var history = JsonSerializer.Deserialize<NestedHistory>(json);
                return history.History;
            }
            return null;
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
