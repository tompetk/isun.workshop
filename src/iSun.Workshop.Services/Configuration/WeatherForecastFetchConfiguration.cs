namespace iSun.Workshop.Services.Configuration
{
    public class WeatherForecastFetchConfiguration
    {
        public WeatherForecastFetchConfiguration(string[] cities, TimeSpan fetchFrequency)
        {
            Cities = cities;
            FetchFrequency = fetchFrequency;
        }

        public string[] Cities { get; }
        public TimeSpan FetchFrequency { get; }
    }
}
