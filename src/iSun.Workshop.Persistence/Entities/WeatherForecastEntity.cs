using Azure;
using Azure.Data.Tables;
using iSun.Workshop.Persistence.Extensions;

namespace iSun.Workshop.Persistence.Entities
{
    public class WeatherForecastEntity : ITableEntity
    {
        public WeatherForecastEntity()
        {
        }

        public WeatherForecastEntity(DateTime fetchedAt, string city, int temperature, int precipitation, double windSpeed, string? summary)
        {
            PartitionKey = GetPartitionKey(city);
            RowKey = GetRowKey(fetchedAt);
            City = city;
            FetchedAt = fetchedAt;
            Temperature = temperature;
            Precipitation = precipitation;
            WindSpeed = windSpeed;
            Summary = summary;
        }

        public static string GetPartitionKey(string city) => city;
        public static string GetRowKey(DateTime time) => time.ToReverseTicks();

        public string City { get; set; }
        public DateTime FetchedAt { get; set; }
        public int Temperature { get; set; }
        public int Precipitation { get; set; }
        public double WindSpeed { get; set; }
        public string? Summary { get; set; }

        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }
}
