namespace iSun.Workshop.Services
{
    public interface IWeatherForecastFetchService
    {
        Task RunFetchAllAsync(CancellationToken cancellationToken = default);
    }
}