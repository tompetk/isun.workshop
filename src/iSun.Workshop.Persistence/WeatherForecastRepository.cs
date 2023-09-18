using Azure.Data.Tables;
using iSun.Workshop.Persistence.Configuration;
using iSun.Workshop.Persistence.Entities;
using iSun.Workshop.Persistence.Interfaces;

namespace iSun.Workshop.Persistence
{
    public class WeatherForecastRepository : IWeatherForecastRepository
    {
        private readonly TableClient _tableClient;

        public WeatherForecastRepository(StorageConfiguration storageConfiguration)
        {
            _tableClient = new TableClient(storageConfiguration.ConnectionString, "WeatherForecasts");
            _tableClient.CreateIfNotExists();
        }

        public Task UpsertAsync(WeatherForecastEntity entity, CancellationToken cancellationToken = default)
        {
            return _tableClient.UpsertEntityAsync(entity, TableUpdateMode.Replace, cancellationToken);
        }
    }
}