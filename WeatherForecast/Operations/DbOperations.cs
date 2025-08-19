using System.Diagnostics.Tracing;
using WeatherForecast.Models;
using WeatherForecast.ViewModels;

namespace WeatherForecast.Operations
{
    public class DbOperations
    {
        private readonly ForecastDbContext _forecast;
        public DbOperations(ForecastDbContext forecast) 
        {
            _forecast = forecast;
        }
        public async Task StoreAndUpdate(AllWeatherNested nested, Task<bool> api)
        {
            var check = await api;
            if (check == true)
            {
                AddLocation(nested.NestedF);
                AddOrUpdateCurrent(nested.NestedF);
                AddOrUpdateHourly(nested.NestedF);
                AddOrUpdateDialy(nested.NestedF);
                AddOrUpdateHistory(nested);
            }
        }

        public void AddOrUpdateDialy(NestedForecast nested)
        {
            var getLocation = GetLocationID(nested);
            var checkID = _forecast.Daily.Any(x => x.LocationID == getLocation);
            if (!checkID)
            {
                AddDaily(nested);
            }
            else
            {
                UpdateDaily(nested);
            }
        }

        public void AddOrUpdateHourly( NestedForecast nested)
        {
            var location = GetLocationID(nested);
            var locationID = _forecast.Hourly.Any(x => x.LocationID == location);
            if (!locationID)
            {
                AddHourly(nested);
            }
            else
            {
                UpdateHourly(nested);
            }
        }

        public void AddOrUpdateCurrent(NestedForecast nested)
        {
            var location = GetLocationID(nested);
            var locationID = _forecast.Current.Any(x => x.LocationID == location);
            if (!locationID)
            {
                AddCurrent(nested);
            }
            else
            {
                UpdateCurrent(nested);
            }
        }

        public void AddLocation(NestedForecast nested)
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

        public void AddOrUpdateHistory(AllWeatherNested nested)
        {
            var location = GetLocationID(nested.NestedF);
            var locationID = _forecast.History.Any(x => x.LocationID == location);
            if (!locationID)
            {
                AddHistory(nested);
            }
            else
            {
                UpdateHistory(nested);
            }
        }
        
        public void AddCurrent(NestedForecast nested)
        {
            var current = nested.CrForecast;
            var currentDb = new Current
            {
                LocationID = GetLocationID(nested),
                Temperature = current.Tempareture,
                WindDirection = current.WeatherCode.ToString(),
                WeatherCode = WeatherCode(current.WeatherCode),
                WindSpeed = current.WindSpeed,
                IsDayTime = current.IsDayTime == 1,
            };
            _forecast.Add(currentDb);
            _forecast.SaveChanges();
        }

        public void UpdateCurrent(NestedForecast nested)
        {
            var locationID = GetLocationID(nested);
            var currentRecords = _forecast.Current.FirstOrDefault(x => x.LocationID == locationID);
            var current = nested.CrForecast;

            currentRecords.Temperature = current.Tempareture;
            currentRecords.WindDirection = current.WeatherCode.ToString();
            currentRecords.WeatherCode = WeatherCode(current.WeatherCode);
            currentRecords.WindSpeed = current.WindSpeed;
            currentRecords.IsDayTime = current.IsDayTime == 1;

            _forecast.SaveChanges();
        }

        public void AddHourly(NestedForecast nested)
        {
            var hourly = nested.HrForecast;
            var getTimes = nested.HrForecast.Time.Select(x => TimeOnly.FromDateTime(x)).ToList();
            for(int i = 0; i < hourly.Time.Count; i++)
            {
                var hourlyDb = new Hourly
                {
                    LocationID = GetLocationID(nested),
                    ForecastTime = getTimes[i],
                    Rain = hourly.Rain[i],
                    Tempareture = hourly.Temp[i]
                };
                _forecast.Hourly.Add(hourlyDb);
          
            }
            _forecast.SaveChanges();
        }

        public void UpdateHourly(NestedForecast nested)
        {
            var hourly = nested.HrForecast;
            var getTimes = hourly.Time.Select(x => TimeOnly.FromDateTime(x)).ToList();
            var locationID = GetLocationID(nested);
            

            for (int i = 0; i < hourly.Time.Count; i++)
            {
                var hourlyRecords = _forecast.Hourly.FirstOrDefault(x => x.LocationID == locationID);

                hourlyRecords.LocationID = locationID;
                hourlyRecords.ForecastTime = getTimes[i];
                hourlyRecords.Rain = hourly.Rain[i];
                hourlyRecords.Tempareture = hourly.Temp[i];
                
            }
            _forecast.SaveChanges();
        }

        public void AddDaily(NestedForecast nested)
        {
            var daily = nested.DlForecast;
            for (int i = 0; i < daily.MaxTempareture.Count; i++)
            {
                var dailyDb = new Daily
                {
                    LocationID = GetLocationID(nested),
                    MaxTempareture = daily.MaxTempareture[i],
                    MinTempareture = daily.MinTempareture[i],
                    RainSum = daily.RainSum[i],
                    WindSpeed = daily.WindSpeed[i],
                    Date = daily.Date[i]
                    
                };
                _forecast.Daily.Add(dailyDb);
            }
            _forecast.SaveChanges();
        }

        public void UpdateDaily(NestedForecast nested)
        {
            var daily = nested.DlForecast;
            var LocationID = GetLocationID(nested);
           
            for (int i = 0; i < daily.MaxTempareture.Count; i++)
            {
                var getDailyRecords = _forecast.Daily.FirstOrDefault(x => x.LocationID == LocationID);

                getDailyRecords.MaxTempareture = daily.MaxTempareture[i];
                getDailyRecords.MinTempareture = daily.MinTempareture[i];
                getDailyRecords.RainSum = daily.RainSum[i];
                getDailyRecords.WindSpeed = daily.WindSpeed[i];
                getDailyRecords.Date = daily.Date[i];
                
            }
            _forecast.SaveChanges();
        }

        public void AddHistory(AllWeatherNested nested)
        {
            var location = GetLocationID(nested.NestedF);
            var history = nested.NestedH.History;
            for (int i = 0; i < history.Temp.Count; i++)
            {
                var historyDb = new History
                {
                    LocationID = location,
                    Tempareture = history.Temp[i],
                    RecordedAt = history.Recorded[i],
                    WindSpeed = history.WindSpeed[i],
                    RainSum = history.Rain[i],
                };
                _forecast.History.Add(historyDb);
            }
            _forecast.SaveChanges();
        }

        public void UpdateHistory(AllWeatherNested nested)
        {
            var locationID = GetLocationID(nested.NestedF);
            var history = nested.NestedH.History;
           
            for (int i = 0; i < history.Temp.Count; i++)
            {
                var historyRecords = _forecast.History.FirstOrDefault(x => x.LocationID == locationID);
                historyRecords.Tempareture = history.Temp[i];
                historyRecords.RecordedAt = history.Recorded[i];
                historyRecords.RainSum = history.Rain[i];
                historyRecords.WindSpeed = history.WindSpeed[i];
                
            }
            _forecast.SaveChanges();
        }

        public int GetLocationID(NestedForecast nested)
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
                case 0:
                    condition = "Mainly clear";
                    break;
                case 1:
                    condition = "Mainly clear";
                    break;
                case 2:
                    condition = "partly cloudy";
                    break;
                case 3:
                    condition = "overcast";
                    break;
                case 45:
                    condition = "Fog";
                    break;
                case 48:
                    condition = "Rime fog";
                    break;
                case 51:
                    condition = "Light Drizzle";
                    break;
                case 53:
                    condition = "Moderate Drizzle";
                    break;
                case 55:
                    condition = "Intense Drizzle";
                    break;
                case 61:
                    condition = "Slight Rain";
                    break;
                case 62:
                    condition = "Moderate Rain";
                    break;
                case 63:
                    condition = "Intense Rain";
                    break;
                case 80:
                    condition = "Slight Rain Showers";
                    break;
                case 81:
                    condition = "Moderate Rain Showers";
                    break;
                case 82:
                    condition = "Intense Rain Showers";
                    break;
                case 95:
                    condition = "Thunder Storms";
                    break;
            }
            return condition;
        }

    }
}
