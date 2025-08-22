using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;
using WeatherForecast.Models;
using WeatherForecast.ViewModels;

namespace WeatherForecast.Operations
{
    public class DbFallback
    {
        private AllWeatherNested nested { get; set; }
        private readonly ForecastDbContext _forecast;

        public DbFallback(ForecastDbContext forecast) 
        {
            _forecast = forecast;
            nested = new AllWeatherNested
            {
                NestedF = new NestedForecast(),
                NestedH = new NestedHistory()
            };
        }
        public List<LocationViewModel> LocationFallback(string name)
        {
            var locationEntities = _forecast.Location
                .Where(x => x.City == name)
                .ToList();

            var locationViewModels = locationEntities.Select(x => new LocationViewModel
            {
                City = x.City,
                Region = x.Region,
                Latitude = x.Latitude,
                Longitude = x.Longitude,

            }).ToList();

            nested.NestedF.Location = locationViewModels;

            return locationViewModels;
        }

        public CurrentViewModel CurrentFallback(string name)
        {
            var locationID = GetLocationID(name);
            var currentTable = _forecast.Current.Where(x => x.LocationID == locationID).FirstOrDefault();
            var currentModel = new CurrentViewModel
            {
                Tempareture = currentTable.Temperature,
                WindDirection = double.Parse(currentTable.WindDirection),
                WindSpeed = currentTable.WindSpeed,
                DayOrNight = IsDayOrNight(currentTable.IsDayTime ? 1 : 0),
                WeatherCodeString = currentTable.WeatherCode,
                WeatherImage = WeatherImageInt(currentTable.WeatherCode)
            };
            nested.NestedF.CrForecast = currentModel;  
            return currentModel;
        }

        public HourlyViewModel HourlyFallback(string name)
        {
            var locationID = GetLocationID(name);
            var hourlyTable = _forecast.Hourly.Where(x => x.LocationID == locationID).ToList();
            var hourlyFallback = new HourlyViewModel
            {
                Rain = hourlyTable.Select(x => x.Rain).ToList(),
                Temp = hourlyTable.Select(x => x.Tempareture).ToList(),
                TimeString = hourlyTable.Select(x => x.ForecastTime.ToString("hh:mm tt")).ToList(),
            };
            nested.NestedF.HrForecast = hourlyFallback;
            return hourlyFallback;
        }

        public DailyViewModel DailyFallback(string name)
        {
            var location = GetLocationID(name);
            var dailyTable = _forecast.Daily.Where(x => x.LocationID == location).ToList();
            var dailyModel = new DailyViewModel
            {
                MaxTempareture = dailyTable.Select(x => x.MaxTempareture).ToList(),
                MinTempareture = dailyTable.Select(x => x.MinTempareture).ToList(),
                RainSum = dailyTable.Select(x => x.RainSum).ToList(),
                WindSpeed = dailyTable.Select(x => x.WindSpeed).ToList(),
                Date = dailyTable.Select(x => x.Date).ToList(),
            };
            nested.NestedF.DlForecast = dailyModel;
            return dailyModel;
        }

        public HistoryViewModel HistoryFallback(string name)
        {
            var location = GetLocationID(name);
            var historyTable = _forecast.History.Where(x => x.LocationID == location).ToList();
            var historyModel = new HistoryViewModel
            {
                Temp = historyTable.Select(x => x.Tempareture).ToList(),
                Rain = historyTable.Select(x => x.RainSum).ToList(),
                WindSpeed = historyTable.Select(x => x.WindSpeed).ToList(),
                Recorded = historyTable.Select(x => x.RecordedAt).ToList(),
            };
            nested.NestedH.History = historyModel;
            return historyModel;
        }

        public int GetLocationID(string name)
        {
            var locationTable = _forecast.Location;
            var locationID = locationTable.Where(x => x.City == name).FirstOrDefault();
            return locationID.LocationID;
        }

        public string IsDayOrNight(int isDay)
        {
            string dayOrNight = isDay == 1 ? "Day" : "Night";
            return dayOrNight;
        }

        public string WeatherImage(int code)
        {
            string condition = "";
            switch (code)
            {
                case 0:
                    condition = "images/sun.png";
                    break;
                case 1:
                    condition = "images/sun.png";
                    break;
                case 2:
                    condition = "images/clear-sky.png";
                    break;
                case 3:
                    condition = "images/clouds.png";
                    break;
                case 45:
                    condition = "images/clouds.png";
                    break;
                case 48:
                    condition = "images/clouds.png";
                    break;
                case 51:
                    condition = "images/rain.png";
                    break;
                case 53:
                    condition = "images/rain.png";
                    break;
                case 55:
                    condition = "images/rain.png";
                    break;
                case 61:
                    condition = "images/rain.png";
                    break;
                case 62:
                    condition = "images/rain.png";
                    break;
                case 63:
                    condition = "images/rain.png";
                    break;
                case 80:
                    condition = "images/rain.png";
                    break;
                case 81:
                    condition = "images/rain.png";
                    break;
                case 82:
                    condition = "images/rain.png";
                    break;
                case 95:
                    condition = "images/rain.png";
                    break;
            }
            return condition;
        }

        public string WeatherImageInt(string code)
        {
            string condition = "";
            switch (code)
            {
                case "clear sky":
                    condition = "images/sun.png";
                    break;
                case "Mainly clear":
                    condition = "images/clear-sky.png";
                    break;
                case "partly cloudy":
                    condition = "images/partly-cloudy.png";
                    break;
                case "overcast":
                    condition = "images/clouds.png";
                    break;
                case "Fog":
                    condition = "images/clouds.png";
                    break;
                case "Rime fog":
                    condition = "images/clouds.png";
                    break;
                case "Light Drizzle":
                    condition = "images/rain.png";
                    break;
                case "Moderate Drizzle":
                    condition = "images/rain.png";
                    break;
                case "Intense Drizzle":
                    condition = "images/rain.png";
                    break;
                case "Slight Rain":
                    condition = "images/rain.png";
                    break;
                case "Moderate Rain":
                    condition = "images/rain.png";
                    break;
                case "Intense Rain":
                    condition = "images/rain.png";
                    break;
                case "Slight Rain Showers":
                    condition = "images/rain.png";
                    break;
                case "Moderate Rain Showers":
                    condition = "images/rain.png";
                    break;
                case "Intense Rain Showers":
                    condition = "images/rain.png";
                    break;
                case "Thunder Storms":
                    condition = "images/rain.png";
                    break;
            }
            return condition;
        }
    }
}
