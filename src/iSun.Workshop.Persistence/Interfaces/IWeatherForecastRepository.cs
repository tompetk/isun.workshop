using iSun.Workshop.Persistence.Entities;

namespace iSun.Workshop.Persistence.Interfaces
{
    public interface IWeatherForecastRepository
    {
        Task UpsertAsync(WeatherForecastEntity entity, CancellationToken cancellationToken = default);
    }
}