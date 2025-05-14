using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Mvc;

namespace SlipStream.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> _logger;

    public WeatherForecastController(ILogger<WeatherForecastController> logger)
    {
        _logger = logger;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public IEnumerable<WeatherForecast> Get()
    {
        
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();
    }


    [HttpGet("stream", Name = "StreamWeatherForecast")]
    public async IAsyncEnumerable<WeatherForecast> StreamGet([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        // int index = 1;
        // while (!cancellationToken.IsCancellationRequested && index <= 5)
        // {
        //     yield return new WeatherForecast
        //     {
        //         Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index++)),
        //         TemperatureC = Random.Shared.Next(-20, 55),
        //         Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        //     };
            
        //     // Simulate delay between items
        //     await Task.Delay(1000);
        // }
        await foreach (var forecast in GetWeatherForecastsAsync(cancellationToken))
        {
            yield return forecast;
            await Task.Delay(1000, cancellationToken); // Simulate delay
        }
    }

    private async IAsyncEnumerable<WeatherForecast> GetWeatherForecastsAsync([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await Task.Delay(10, cancellationToken); // Simulate initial delay
        foreach (var summary in Summaries)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                yield break; // Stop if cancellation is requested
            }
            yield return new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = summary
            };
        }
    }
}
