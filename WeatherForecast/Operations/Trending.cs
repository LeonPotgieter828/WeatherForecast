using WeatherForecast.ViewModels;

namespace WeatherForecast.Operations
{
    public class Trending
    {

        public List<double> TempAvg { get; set; }
        public List<double> WindAvg { get; set; }
        public double WeeklyMaxTemp { get; set; } 
        public double WeeklyMinTemp { get; set; } 
        public double WeeklyWind { get; set; } 

        public Trending() 
        {
            TempAvg = new List<double>();
            WindAvg = new List<double>();
        }

        public void Averages(AllWeatherNested nested)
        {
            AverageTemp(nested);
            AverageWind(nested);
            WeeklyWindAvg(nested);
            WeeklyMaxTempAvg(nested);
            WeeklyMinTempAvg(nested);
        }

        public List<double> AverageTemp(AllWeatherNested nested)
        {
            double total = 0;
            var daily = nested.NestedF.DlForecast;
            var history = nested.NestedH.History;

            for (int i = 0; i < 5; i++)
            {
                total = (daily.MaxTempareture[i] + history.Temp[i+1]) / 2;
                TempAvg.Add(Round(total));
            }

            return TempAvg;
        }
        
        public List<double> AverageWind(AllWeatherNested nested)
        {
            double total = 0;
            var dialy = nested.NestedF.DlForecast;
            var history = nested.NestedH.History;

            for (int i = 0; i < history.WindSpeed.Count; i++)
            {
                total = (dialy.WindSpeed[i+1] + history.WindSpeed[i]) / 2;
                WindAvg.Add(Round(total));
            }

            return WindAvg;
        }
        
        public double WeeklyWindAvg(AllWeatherNested nested)
        {
            double total = 0;
            var daily = nested.NestedF.DlForecast;
            WeeklyWind = daily.WindSpeed.Take(7).Average();
            return WeeklyWind;
        }
        public double WeeklyMaxTempAvg(AllWeatherNested nested)
        {
            double total = 0;
            var daily = nested.NestedF.DlForecast;
            WeeklyMaxTemp = daily.MaxTempareture.Take(7).Average();
            return WeeklyMaxTemp;
        }
        
        public double WeeklyMinTempAvg(AllWeatherNested nested)
        {
            double total = 0;
            var daily = nested.NestedF.DlForecast;
            WeeklyMinTemp = daily.MinTempareture.Take(7).Average();
            return WeeklyMinTemp;
        }


        public double Round(double total)
        {
            double round = Math.Round(total, 0);
            return round;
        }


    }
}
