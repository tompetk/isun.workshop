namespace iSun.Workshop.Clients.DTOs
{
    public class WeatherForecastResponseDTO
    {
        public WeatherForecastResponseDTO()
        {
        }

        public WeatherForecastResponseDTO(string? city, int temperature, int precipitation, double windSpeed, string? summary)
        {
            City = city;
            Temperature = temperature;
            Precipitation = precipitation;
            WindSpeed = windSpeed;
            Summary = summary;
        }

        public string? City { get; set; }
        public int Temperature { get; set; }
        public int Precipitation { get; set; }
        public double WindSpeed { get; set; }
        public string? Summary { get; set; }
    }
}
