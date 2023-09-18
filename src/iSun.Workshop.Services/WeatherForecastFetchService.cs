using iSun.Workshop.Clients.Interfaces;
using iSun.Workshop.Persistence.Entities;
using iSun.Workshop.Persistence.Interfaces;
using iSun.Workshop.Services.Configuration;
using Microsoft.Extensions.Logging;

namespace iSun.Workshop.Services
{
    public class WeatherForecastFetchService : IWeatherForecastFetchService
    {
        private readonly TimeSpan _fetchFrequency;
        private readonly string[] _cities;
        private readonly ILogger _logger;
        private readonly IWeatherForecastClient _weatherForecastClient;
        private readonly IWeatherForecastRepository _weatherForecastRepository;

        public WeatherForecastFetchService(
            ILogger<WeatherForecastFetchService> logger,
            IWeatherForecastClient weatherForecastClient,
            IWeatherForecastRepository weatherForecastRepository,
            WeatherForecastFetchConfiguration configuration)
        {
            _logger = logger;
            _weatherForecastClient = weatherForecastClient;
            _weatherForecastRepository = weatherForecastRepository;
            _fetchFrequency = configuration.FetchFrequency;
            _cities = configuration.Cities ?? throw new ArgumentNullException(nameof(configuration.Cities));
        }

        public async Task RunFetchAllAsync(CancellationToken cancellationToken = default)
        {
            var validCities = await GetValidCitiesAsync(cancellationToken);
            if (validCities.Count == 0)
                throw new Exception("No supported cities to fetch weather forecasts for.");

            var periodicTimer = new PeriodicTimer(_fetchFrequency);
            while (await periodicTimer.WaitForNextTickAsync(cancellationToken))
            {
                // TODO: use SemaphoreSlim if many cities, to avoid API throttling.
                var forecastFetchTasks = validCities
                    .Select(cityName => FetchCityAsync(cityName, cancellationToken));

                try
                {
                    await Task.WhenAll(forecastFetchTasks);

                    _logger.LogInformation("Forecasts fetched successfully.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Forecast fetching failed for some of the cities.");
                }
            }
        }

        private async Task<List<string>> GetValidCitiesAsync(CancellationToken cancellationToken)
        {
            var supportedCities = await _weatherForecastClient.GetCitiesAsync(cancellationToken);

            // Print not valid cities.
            var notValidCities = _cities.Where(c => 
                !supportedCities.Any(_c => _c.ToLowerInvariant() == c.ToLowerInvariant())).ToList();
            var notValidCitiesList = string.Join(", ", notValidCities);
            if (notValidCitiesList.Length > 0)
                _logger.LogWarning($"Following cities are not supported: {notValidCitiesList}. Ignoring.");

            // Filter out supported cities.
            var validCities = _cities.Where(c => 
                supportedCities.Any(_c => _c.ToLowerInvariant() == c.ToLowerInvariant())).ToList();

            return validCities;
        }

        private async Task FetchCityAsync(string cityName, CancellationToken cancellationToken)
        {
            var fetchedAt = DateTime.UtcNow;

            // Fetch forecast.
            var forecast = await _weatherForecastClient.GetForecastAsync(cityName, cancellationToken);

            // Log.
            _logger.LogInformation($"{cityName}\tT: {forecast.Temperature}\tP: {forecast.Precipitation}\tWS: {forecast.WindSpeed}\tSummary: {forecast.Summary}");

            // Persist.
            var entity = new WeatherForecastEntity(
                fetchedAt, cityName, forecast.Temperature,
                forecast.Precipitation, forecast.WindSpeed, forecast.Summary);

            await _weatherForecastRepository.UpsertAsync(entity, cancellationToken);
        }
    }
}