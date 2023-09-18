using iSun.Workshop.Clients.Configuration;
using iSun.Workshop.Clients.DTOs;
using iSun.Workshop.Clients.Interfaces;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Text.Json;

namespace iSun.Workshop.Clients
{
    public class WeatherForecastClient : IWeatherForecastClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiBaseUrl;
        private readonly string _apiUsername;
        private readonly string _apiPassword;
        private string? _accessToken;

        public WeatherForecastClient(
            IHttpClientFactory httpClientFactory,
            WeatherForecastClientConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _apiBaseUrl = configuration.ApiUrl ?? throw new ArgumentNullException(nameof(configuration.ApiUrl));
            _apiUsername = configuration.ApiUsername ?? throw new ArgumentNullException(nameof(configuration.ApiUsername));
            _apiPassword = configuration.ApiPassword ?? throw new ArgumentNullException(nameof(configuration.ApiPassword));
        }

        /// <summary>
        /// Authorizes the client. Must be done before using it.
        /// </summary>
        public async Task AuthorizeAsync(CancellationToken cancellationToken = default)
        {
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri($"{_apiBaseUrl}/api/");

            var request = new HttpRequestMessage(HttpMethod.Post, "authorize")
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(new AuthorizeRequestDTO(_apiUsername, _apiPassword)),
                    System.Text.Encoding.UTF8,
                    MediaTypeNames.Application.Json)
            };

            var response = await client.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var responseDto = await response.Content.ReadFromJsonAsync<AuthorizeResponseDTO>(cancellationToken: cancellationToken);
            _accessToken = responseDto!.Token;
        }

        /// <summary>
        /// Fetches list of cities forecasts are available for.
        /// </summary>
        /// <returns>List of city names.</returns>
        public async Task<List<string>> GetCitiesAsync(CancellationToken cancellationToken = default)
        {
            await EnsureAuthorizationAsync();

            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri($"{_apiBaseUrl}/api/");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

            var request = new HttpRequestMessage(HttpMethod.Get, "cities");
            var response = await client.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var cityNames = await response.Content.ReadFromJsonAsync<List<string>>(cancellationToken: cancellationToken);
            return cityNames;
        }

        /// <summary>
        /// Fetches forecasts for the specified city.
        /// </summary>
        /// <param name="city">Name of the city (returned by <see cref="GetCitiesAsync(CancellationToken)" />). </param>
        /// <returns>Weather forecast.</returns>
        public async Task<WeatherForecastResponseDTO> GetForecastAsync(string city, CancellationToken cancellationToken = default)
        {
            await EnsureAuthorizationAsync();

            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri($"{_apiBaseUrl}/api/");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

            // TODO: retry policy for TooManyRequests

            var request = new HttpRequestMessage(HttpMethod.Get, $"weathers/{city}");
            var response = await client.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var forecastDto = await response.Content.ReadFromJsonAsync<WeatherForecastResponseDTO>(cancellationToken: cancellationToken);
            return forecastDto!;
        }

        private Task EnsureAuthorizationAsync()
        {
            if (_accessToken != null) return Task.CompletedTask;
            return AuthorizeAsync();
        }
    }
}