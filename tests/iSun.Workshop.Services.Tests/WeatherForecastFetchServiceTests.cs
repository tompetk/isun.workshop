using iSun.Workshop.Clients.DTOs;
using iSun.Workshop.Clients.Interfaces;
using iSun.Workshop.Persistence.Entities;
using iSun.Workshop.Persistence.Interfaces;
using iSun.Workshop.Services.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace iSun.Workshop.Services.Tests
{
    [TestClass]
    public class WeatherForecastFetchServiceTests
    {
        const string ExistingCity1 = "Vilnius";
        const string ExistingCity2 = "Kaunas";
        const string MissingCity = "Palanga";

        private readonly TimeSpan _fetchFrequency = TimeSpan.FromSeconds(2);

        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly CancellationToken _cancellationToken;

        private readonly WeatherForecastFetchService _service;

        private readonly Mock<IWeatherForecastClient> _weatherForecastClientMock;

        private readonly WeatherForecastResponseDTO _vilniusForecast1 = new WeatherForecastResponseDTO(ExistingCity1, 20, 80, 16, "Windy");
        private readonly WeatherForecastResponseDTO _vilniusForecast2 = new WeatherForecastResponseDTO(ExistingCity1, 24, 85, 12, "Windy");
        private readonly WeatherForecastResponseDTO _kaunasForecast1 = new WeatherForecastResponseDTO(ExistingCity1, 18, 70, 14, "Windy");
        private readonly WeatherForecastResponseDTO _kaunasForecast2 = new WeatherForecastResponseDTO(ExistingCity1, 20, 60, 15, "Windy");
        private readonly Mock<IWeatherForecastRepository> _weatherForecastRepositoryMock;

        public WeatherForecastFetchServiceTests()
        {
            _cancellationToken = _cancellationTokenSource.Token;
            var loggerMock = new Mock<ILogger<WeatherForecastFetchService>>();

            var supportedCities = new List<string> { ExistingCity1, ExistingCity2 };
            _weatherForecastClientMock = new Mock<IWeatherForecastClient>();
            _weatherForecastClientMock
                .Setup(m => m.GetCitiesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(supportedCities);

            _weatherForecastRepositoryMock = new Mock<IWeatherForecastRepository>();
            var configuration = new WeatherForecastFetchConfiguration(
                supportedCities.ToArray(), _fetchFrequency);

            _service = new WeatherForecastFetchService(
                loggerMock.Object, 
                _weatherForecastClientMock.Object, 
                _weatherForecastRepositoryMock.Object, 
                configuration);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception), "No supported cities to fetch weather forecasts for.")]
        public async Task RunFetchAllAsync_NoSupportedCities_ShouldFail()
        {
            // Arrange
            _weatherForecastClientMock
                .Setup(m => m.GetCitiesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<string>() { MissingCity });

            // Act
            await _service.RunFetchAllAsync(_cancellationToken);
        }

        [TestMethod]
        public async Task RunFetchAllAsync_ExistingCity_ShouldSucceed()
        {
            _weatherForecastClientMock
                .SetupSequence(m => m.GetForecastAsync(ExistingCity1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(_vilniusForecast1)
                .ReturnsAsync(_vilniusForecast2);

            _weatherForecastClientMock
                .SetupSequence(m => m.GetForecastAsync(ExistingCity2, It.IsAny<CancellationToken>()))
                .ReturnsAsync(_kaunasForecast1)
                .ReturnsAsync(_kaunasForecast2);

            _ = _service.RunFetchAllAsync(_cancellationToken);

            await Task.Delay(_fetchFrequency * 2.5); // wait for 2 fetch cycles and cancel running.
            _cancellationTokenSource.Cancel();

            // Assert all 4 forecasts were saved for 2 cities.
            _weatherForecastRepositoryMock.Verify(m =>
                m.UpsertAsync(
                    It.Is<WeatherForecastEntity>(e => 
                        e.City == ExistingCity1 && e.Temperature == _vilniusForecast1.Temperature &&
                        e.Precipitation == _vilniusForecast1.Precipitation && e.WindSpeed == _vilniusForecast1.WindSpeed),
                    _cancellationToken), Times.Once);

            _weatherForecastRepositoryMock.Verify(m =>
                m.UpsertAsync(
                    It.Is<WeatherForecastEntity>(e =>
                        e.City == ExistingCity1 && e.Temperature == _vilniusForecast2.Temperature &&
                        e.Precipitation == _vilniusForecast2.Precipitation && e.WindSpeed == _vilniusForecast2.WindSpeed),
                    _cancellationToken), Times.Once);

            _weatherForecastRepositoryMock.Verify(m =>
                m.UpsertAsync(
                    It.Is<WeatherForecastEntity>(e =>
                        e.City == ExistingCity2 && e.Temperature == _kaunasForecast1.Temperature &&
                        e.Precipitation == _kaunasForecast1.Precipitation && e.WindSpeed == _kaunasForecast1.WindSpeed),
                    _cancellationToken), Times.Once);

            _weatherForecastRepositoryMock.Verify(m =>
                m.UpsertAsync(
                    It.Is<WeatherForecastEntity>(e =>
                        e.City == ExistingCity2 && e.Temperature == _kaunasForecast2.Temperature &&
                        e.Precipitation == _kaunasForecast2.Precipitation && e.WindSpeed == _kaunasForecast2.WindSpeed),
                    _cancellationToken), Times.Once);
        }

        [TestMethod]
        public async Task RunFetchAllAsync_FirstFetchFails_SecondShouldSucceed()
        {
            _weatherForecastClientMock
                .SetupSequence(m => m.GetForecastAsync(ExistingCity1, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new HttpRequestException())
                .ReturnsAsync(_vilniusForecast2);

            _weatherForecastClientMock
                .SetupSequence(m => m.GetForecastAsync(ExistingCity2, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new HttpRequestException())
                .ReturnsAsync(_kaunasForecast2);

            _ = _service.RunFetchAllAsync(_cancellationToken);

            await Task.Delay(_fetchFrequency * 2.5); // wait for 2 fetch cycles and cancel running.
            _cancellationTokenSource.Cancel();

            // Assert 2 forecasts were saved for 2 cities.
            _weatherForecastRepositoryMock.Verify(m =>
                m.UpsertAsync(
                    It.Is<WeatherForecastEntity>(e =>
                        e.City == ExistingCity1 && e.Temperature == _vilniusForecast2.Temperature &&
                        e.Precipitation == _vilniusForecast2.Precipitation && e.WindSpeed == _vilniusForecast2.WindSpeed),
                    _cancellationToken), Times.Once);

            _weatherForecastRepositoryMock.Verify(m =>
                m.UpsertAsync(
                    It.Is<WeatherForecastEntity>(e =>
                        e.City == ExistingCity2 && e.Temperature == _kaunasForecast2.Temperature &&
                        e.Precipitation == _kaunasForecast2.Precipitation && e.WindSpeed == _kaunasForecast2.WindSpeed),
                    _cancellationToken), Times.Once);
        }
    }
}