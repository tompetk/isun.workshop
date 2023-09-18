using CommandLine;
using iSun.Workshop.Clients;
using iSun.Workshop.Clients.Interfaces;
using iSun.Workshop.Persistence;
using iSun.Workshop.Persistence.Interfaces;
using iSun.Workshop.Services;
using iSun.Workshop.Services.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using iSun.Workshop.Clients.Configuration;
using iSun.Workshop.Persistence.Configuration;

namespace iSun.Workshop
{
    class Options
    {
        [Option('c', "cities", Required = true, HelpText = "Cities to fetch forecasts for separated with comma.\r\nE.g.: --cities Vilnius,Kaunas")]
        public IEnumerable<string> Cities { get; set; }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            var options = CommandLine.Parser.Default.ParseArguments<Options>(args);
            if (options.Errors.Any())
                throw new ArgumentException("Invalid arguments.");

            // TODO: make configurable the following via appsettings.json or args.
            const string storageConnectionString = "DefaultEndpointsProtocol=https;AccountName=tpisunworkshop23;AccountKey=Q6Be8CVFAyI2bo0THoe7wLgJfQqZGdZFniT07h40jeC8zck5rF/ooASdqNrJL3pANq3CQgvSBt+r+AStyQOFPw==;EndpointSuffix=core.windows.net";
            var fetchFrequency = TimeSpan.FromSeconds(15);
            const string apiUrl = "https://weather-api.isun.ch";
            const string apiUsername = "isun";
            const string apiPassword = "passwrod";

            var citiesArray = options.Value.Cities
                .Select(c => c.Replace(",", "").Trim())
                .ToArray();

            var serviceCollection = new ServiceCollection()
                .AddLogging(configure => 
                    configure
                        .AddSystemdConsole(c => c.TimestampFormat = "[yyyy.MM.dd HH:mm:ss] ")
                        .AddFilter((cat, level) => cat.StartsWith(typeof(Program).Namespace!))
                        .SetMinimumLevel(LogLevel.Warning))
                .AddHttpClient()
                .AddSingleton(new StorageConfiguration(storageConnectionString))
                .AddSingleton(new WeatherForecastClientConfiguration(apiUrl, apiUsername, apiPassword))
                .AddSingleton(new WeatherForecastFetchConfiguration(citiesArray, fetchFrequency))
                .AddSingleton<IWeatherForecastClient, WeatherForecastClient>()
                .AddSingleton<IWeatherForecastRepository, WeatherForecastRepository>()
                .AddSingleton<IWeatherForecastFetchService, WeatherForecastFetchService>();

            var provider = serviceCollection.BuildServiceProvider();

            var weatherForecastFetchService = provider.GetRequiredService<IWeatherForecastFetchService>();
            
            // Just start without awaiting to enable user input.
            _ = weatherForecastFetchService.RunFetchAllAsync();

            Console.WriteLine("Weather forecasts fetching started...");
            Console.Read(); // TODO: add support for stop option.
        }
    }
}