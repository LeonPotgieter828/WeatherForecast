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
        public string url { get; set; }
        public string TempLocation { get; set; }

        string message = "";

        private readonly ILogger<IndexModel> _logger;

        private static readonly HttpClient _httpClient = new HttpClient();

        public AllWeatherNested nested { get; set; }
        public DbOperations _db { get; set; }
        public DbFallback _fallback { get; set; }
        public Task<bool> ApiSuccess { get; set; }
        public Trending _trend { get; set; }
        public ApiOperations _api { get; set; }

        public ForecastDbContext _forecast;

        public IndexModel(ILogger<IndexModel> logger, ForecastDbContext forecast)
        {
            _logger = logger;
            _forecast = forecast;
            _db = new DbOperations(_forecast);
            _fallback = new DbFallback(_forecast);
            _api = new ApiOperations(_forecast);
        }

        public async Task OnGet(string searchName)
        {
                SearchName = string.IsNullOrWhiteSpace(searchName) ? "Port Elizabeth" : searchName;
                nested = await BuildForecast(SearchName);
                ViewData["message"] = message;
        }

        public async Task<IActionResult> OnPost()
        {      
            nested = await BuildForecast(SearchName);
            ViewData["message"] = message;
            return Page();
        }

        public Trending TrendBuilder(AllWeatherNested nested)
        {
            var tren = new Trending();
            tren.Averages(nested);
            return tren;
        }

        private async Task<AllWeatherNested> BuildForecast(string searchName)
        {
            if (searchName.IsNullOrEmpty())
            {
                SearchName = "Port Elizabeth";
                searchName = SearchName;
            }
            var nes = new AllWeatherNested
            {
                NestedF = new NestedForecast
                {
                    Location = await LocationSearch(searchName),
                    CrForecast = await _api.CurrentWeather(TempLocation),
                    HrForecast = await _api.HourlyWeather(TempLocation),
                    DlForecast = await _api.DailyWeather(TempLocation),
                },

                NestedH = new NestedHistory
                {
                    History = await _api.HistoryForecast(TempLocation)
                }
            };
            ApiSuccess = _api.ApiResponse(url);
            _trend = TrendBuilder(nes);
            await _db.StoreAndUpdate(nes, ApiSuccess);
            return nes;
        }

        public async Task<string> LocationApi(string name)
        {
            TempLocation = TempData["LastStored"]?.ToString();
            TempData.Keep("LastStored");

            url = $"https://geocoding-api.open-meteo.com/v1/search?name={name}&count=1&language=en&format=json";
            var getResponse = await _httpClient.GetAsync(url);

                if (getResponse.IsSuccessStatusCode)
                {
                    var json = await getResponse.Content.ReadAsStringAsync();
                    var location = JsonSerializer.Deserialize<NestedForecast>(json);
                    if (location.Location != null)
                    {
                        TempData["LastStored"] = name;
                        message = "";
                        return url;
                    }
                    else
                    {
                        message = "Location was not found";                      
                        url = $"https://geocoding-api.open-meteo.com/v1/search?name={TempLocation}&count=1&language=en&format=json";
                    }
                }            
            return url;
        }

        public async Task<List<LocationViewModel>> LocationSearch(string name)
        {
            try
            {
                url = await LocationApi(name);
                var getResponse = await _httpClient.GetAsync(url);
                if (getResponse.IsSuccessStatusCode)
                {
                    var json = await getResponse.Content.ReadAsStringAsync();
                    var location = JsonSerializer.Deserialize<NestedForecast>(json);

                    var getLocation = location.Location.FirstOrDefault();
                    longitude = getLocation.Longitude;
                    latitude = getLocation.Latitude;
                    return location.Location;
                }
                return _fallback.LocationFallback(TempLocation);
            }
            catch (TaskCanceledException)
            {
                return _fallback.LocationFallback(TempLocation);
            }
        }


        

        
    }
}
