using iSun.Workshop.Clients.DTOs;

namespace iSun.Workshop.Clients.Interfaces
{
    public interface IWeatherForecastClient
    {
        Task AuthorizeAsync(CancellationToken cancellationToken = default);
        Task<List<string>> GetCitiesAsync(CancellationToken cancellationToken = default);
        Task<WeatherForecastResponseDTO> GetForecastAsync(string city, CancellationToken cancellationToken = default);
    }
}