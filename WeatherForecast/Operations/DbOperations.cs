using System.Diagnostics.Tracing;
using WeatherForecast.Models;
using WeatherForecast.ViewModels;

namespace WeatherForecast.Operations
{
    public class DbOperations
    {

        public void AddOrUpdateDialy(ForecastDbContext _forecast, NestedForecast nested)
        {
            var getLocation = GetLocationID(_forecast, nested);
            var checkID = _forecast.Daily.Any(x => x.LocationID == getLocation);
            if (!checkID)
            {
                AddDaily(_forecast, nested);
            }
            else
            {
                UpdateDaily(_forecast, nested);
            }
        }

        public bool AddOrUpdateHourly(ForecastDbContext _forecast, NestedForecast nested)
        {
            var location = GetLocationID(_forecast, nested);
            var locationID = _forecast.Hourly.Any(x => x.LocationID == location);
            if (!locationID)
            {
                AddHourly(_forecast, nested);
            }
            else
            {
                UpdateHourly(_forecast, nested);
            }
            return !locationID;
        }


        public void AddLocation(ForecastDbContext _forecast, NestedForecast nested)
        {
            var location = nested.Location.FirstOrDefault();
            var locationDb = new Location
            {
                Latitude = location.Latitude,
                Longitude = location.Longitude,
                Region = location.Region,
                City = location.City,
            };
            var distinct = _forecast.Location.Any(x => x.City == locationDb.City);
            if (!distinct)
            {
                _forecast.Add(locationDb);
            }
            _forecast.SaveChanges();
        }
        
        public void AddCurrent(ForecastDbContext _forecast, NestedForecast nested)
        {
            var current = nested.CrForecast;
            var currentDb = new Current
            {
                LocationID = GetLocationID(_forecast, nested),
                Temperature = current.Tempareture,
                WindDirection = current.WeatherCode.ToString(),
                WeatherCode = WeatherCode(current.WeatherCode),
                WindSpeed = current.WindSpeed,
                IsDayTime = current.IsDayTime == 1,
            };
            _forecast.Add(currentDb);
            _forecast.SaveChanges();
        }

        public void AddHourly(ForecastDbContext _forecast, NestedForecast nested)
        {
            var hourly = nested.HrForecast;
            var getTimes = nested.HrForecast.Time.Select(x => TimeOnly.FromDateTime(x)).ToList();
            for(int i = 0; i < hourly.Time.Count; i++)
            {
                var hourlyDb = new Hourly
                {
                    LocationID = GetLocationID(_forecast, nested),
                    ForecastTime = getTimes[i],
                    Rain = hourly.Rain[i],
                    Tempareture = hourly.Temp[i]
                };
                _forecast.Hourly.Add(hourlyDb);
          
            }
            _forecast.SaveChanges();
        }

        public void UpdateHourly(ForecastDbContext _forecast, NestedForecast nested)
        {
            var hourly = nested.HrForecast;
            var getTimes = hourly.Time.Select(x => TimeOnly.FromDateTime(x)).ToList();
            var locationID = GetLocationID(_forecast, nested);
            var hourlyRecords = _forecast.Hourly.Any(x => x.LocationID == locationID);

            for (int i = 0; i < hourly.Time.Count; i++)
            {
                var hourlyDb = new Hourly
                {
                    LocationID = locationID,
                    ForecastTime = getTimes[i],
                    Rain = hourly.Rain[i],
                    Tempareture = hourly.Temp[i]
                };
            }
            _forecast.SaveChanges();
        }

        


        public void AddDaily(ForecastDbContext _forecast, NestedForecast nested)
        {
            var daily = nested.DlForecast;
            for (int i = 0; i < daily.MaxTempareture.Count; i++)
            {
                var dailyDb = new Daily
                {
                    LocationID = GetLocationID(_forecast, nested),
                    MaxTempareture = daily.MaxTempareture[i],
                    MinTempareture = daily.MinTempareture[i],
                    RainSum = daily.RainSum[i],
                    WindSpeed = daily.WindSpeed[i]
                };
                _forecast.Daily.Add(dailyDb);
            }
            _forecast.SaveChanges();
        }

        public void UpdateDaily(ForecastDbContext _forecast, NestedForecast nested)
        {
            var daily = nested.DlForecast;
            var LocationID = GetLocationID(_forecast, nested);
            var getDailyRecords = _forecast.Daily.Where(x => x.LocationID == LocationID).ToList();

            for (int i = 0; i < getDailyRecords.Count; i++)
            {
                var dailyDb = new Daily
                {
                    MaxTempareture = daily.MaxTempareture[i],
                    MinTempareture = daily.MinTempareture[i],
                    RainSum = daily.RainSum[i],
                    WindSpeed = daily.WindSpeed[i]
                };
            }
            _forecast.SaveChanges();
        }




        public int GetLocationID(ForecastDbContext _forecast, NestedForecast nested)
        {
            var location = nested.Location.FirstOrDefault();
            var locationID = _forecast.Location.Where(x => x.City == location.City).FirstOrDefault();
            return locationID.LocationID;   
        }

        public string WeatherCode(int code)
        {
            string condition = "";
            switch (code)
            {
                case 1:
                    condition = "Mainly clear";
                    break;
                case 2:
                    condition = "partly cloudy";
                    break;
                case 3:
                    condition = "overcast";
                    break;
            }
            return condition;
        }

    }
}
