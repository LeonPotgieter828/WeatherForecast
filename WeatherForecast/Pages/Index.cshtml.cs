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
        public string TempLocation { get; set; }

        public AllWeatherNested nested { get; set; }
        public DbOperations _db { get; set; }
        public DbFallback _fallback { get; set; }
        public Task<bool> ApiSuccess { get; set; }
        public Trending _trend { get; set; }
        public ApiOperations _api { get; set; }

        public ForecastDbContext _forecast;
        private static readonly HttpClient _httpClient = new HttpClient();
        private readonly ILogger<IndexModel> _logger;
        string message = "";

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
            ApiSuccess = _api.ApiResponse(await LocationApi(searchName));
            await LocationApi(searchName);

            var nes = new AllWeatherNested
            {
                NestedF = new NestedForecast
                {
                    Location = await _api.LocationSearch(TempLocation, await LocationApi(searchName), await ApiSuccess),
                    CrForecast = await _api.CurrentWeather(TempLocation, await ApiSuccess),
                    HrForecast = await _api.HourlyWeather(TempLocation, await ApiSuccess),
                    DlForecast = await _api.DailyWeather(TempLocation, await ApiSuccess),
                },

                NestedH = new NestedHistory
                {
                    History = await _api.HistoryForecast(TempLocation, await ApiSuccess)
                }
            };
            
            _trend = TrendBuilder(nes);
            await _db.StoreAndUpdate(nes, ApiSuccess);
            return nes;
        }

        public async Task<string> LocationApi(string name)
        {
            TempLocation = TempData["LastStored"]?.ToString();
            TempData.Keep("LastStored");
            var checkDbLocation = _forecast.Location.Any(x => x.City == name);

            string url = $"https://geocoding-api.open-meteo.com/v1/search?name={name}&count=1&language=en&format=json";
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
            else if(checkDbLocation)
            {
                TempLocation = name;
                TempData["LastStored"] = name;
            }            
            return url;
        }
      
    }
}
