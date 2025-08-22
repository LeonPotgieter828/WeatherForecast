using System.Net.Http;
using System;
using WeatherForecast.ViewModels;
using System.Text.Json;
using System.Globalization;
using WeatherForecast.Models;

namespace WeatherForecast.Operations
{
    public class ApiOperations
    {
        public string SearchName { get; set; }
        public double longitude { get; set; }
        public double latitude { get; set; }
        public string url { get; set; }
        public DbOperations _db { get; set; }
        public DbFallback _fallback { get; set; }

        public ForecastDbContext _forecast;

        private static readonly HttpClient _httpClient = new HttpClient();

        public ApiOperations(ForecastDbContext forecast)
        {
            _forecast = forecast;
            _db = new DbOperations(_forecast);
            _fallback = new DbFallback(_forecast);
        }

        public async Task<List<LocationViewModel>> LocationSearch(string TempLocation, string locationUrl, bool api)
        {
            try
            {
                var url = locationUrl;
                var getResponse = await _httpClient.GetAsync(url);
                if (getResponse.IsSuccessStatusCode && api)
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

        public async Task<CurrentViewModel?> CurrentWeather(string TempLocation, bool apiResponse)
        {
            try
            {
                var getResponse = await _httpClient.GetAsync(ForecastURL());
                if (getResponse.IsSuccessStatusCode && apiResponse)
                {
                    var json = await getResponse.Content.ReadAsStringAsync();
                    var currentWeather = JsonSerializer.Deserialize<NestedForecast>(json);
                    currentWeather.CrForecast.DayOrNight = _fallback.IsDayOrNight(currentWeather.CrForecast.IsDayTime);
                    currentWeather.CrForecast.WeatherCodeString = _db.WeatherCode(currentWeather.CrForecast.IsDayTime);
                    currentWeather.CrForecast.WeatherImage = _fallback.WeatherImage(currentWeather.CrForecast.WeatherCode);
                    return currentWeather.CrForecast;
                }
                return _fallback.CurrentFallback(TempLocation);
            }
            catch (TaskCanceledException)
            {
                return _fallback.CurrentFallback(TempLocation);
            }
        }

        public async Task<HourlyViewModel> HourlyWeather(string TempLocation, bool apiResponse)
        {
            try
            {
                var getResponse = await _httpClient.GetAsync(ForecastURL());
                if (getResponse.IsSuccessStatusCode && apiResponse)
                {
                    var json = await getResponse.Content.ReadAsStringAsync();
                    var hourlyWeather = JsonSerializer.Deserialize<NestedForecast>(json);
                    var today = hourlyWeather.HrForecast.Time.Where(x => x.Date == DateTime.Today.Date).ToList();
                    hourlyWeather.HrForecast.TimeString = today.Select(t => t.ToString("hh:mm")).ToList();
                    return Hours(hourlyWeather);
                }
                return _fallback.HourlyFallback(TempLocation);
            }
            catch (TaskCanceledException)
            {
                return _fallback.HourlyFallback(TempLocation);
            }
        }
        public async Task<DailyViewModel> DailyWeather(string TempLocation, bool apiResponse)
        {
            try
            {
                var getResponse = await _httpClient.GetAsync(ForecastURL());
                if (getResponse.IsSuccessStatusCode && apiResponse)
                {
                    var json = await getResponse.Content.ReadAsStringAsync();
                    var dailyWeather = JsonSerializer.Deserialize<NestedForecast>(json);
                    return dailyWeather.DlForecast;
                }
                return _fallback.DailyFallback(TempLocation);
            }
            catch (TaskCanceledException)
            {
                return _fallback.DailyFallback(TempLocation);
            }
        }

        public async Task<HistoryViewModel> HistoryForecast(string TempLocation, bool apiResponse)
        {
            try
            {
                var getResponse = await _httpClient.GetAsync(HistoryURL());
                if (getResponse.IsSuccessStatusCode && apiResponse)
                {
                    var json = await getResponse.Content.ReadAsStringAsync();
                    var history = JsonSerializer.Deserialize<NestedHistory>(json);
                    history.History.Recorded = history.History.Recorded.OrderByDescending(x => x.Day).ToList();
                    return history.History;
                }
                return _fallback.HistoryFallback(TempLocation);
            }
            catch (TaskCanceledException)
            {
                return _fallback.HistoryFallback(TempLocation);
            }
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

        public async Task<bool> ApiResponse(string url)
        {
            try
            {
                var getForecastApi = await _httpClient.GetAsync(ForecastURL());
                var getHistoryApi = await _httpClient.GetAsync(HistoryURL());
                var getLocationApi = await _httpClient.GetAsync(url);
                if (getForecastApi.IsSuccessStatusCode && getHistoryApi.IsSuccessStatusCode && getLocationApi.IsSuccessStatusCode)
                {
                    return true;
                }
                return false;
            }
            catch (TaskCanceledException)
            {
                return false;
            }
        }

    }
}
