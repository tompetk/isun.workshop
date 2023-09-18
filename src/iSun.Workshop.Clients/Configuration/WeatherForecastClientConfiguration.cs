namespace iSun.Workshop.Clients.Configuration
{
    public class WeatherForecastClientConfiguration
    {
        public WeatherForecastClientConfiguration(string apiUrl, string apiUsername, string apiPassword)
        {
            ApiUrl = apiUrl;
            ApiUsername = apiUsername;
            ApiPassword = apiPassword;
        }

        public string ApiUrl { get; }
        public string ApiUsername { get; }
        public string ApiPassword { get; }
    }
}
