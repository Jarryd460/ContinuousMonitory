using Microsoft.AspNetCore.Mvc;
using OpenTelemetry.Trace;
using System.Diagnostics;

namespace ContinuousMonitory.Controllers
{
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
            _logger.LogInformation("Retrieving weather forecasts");

            // Starts a trace
            using var activity = DiagnosticConfiguration.ActivitySource.StartActivity(DiagnosticConfiguration.SourceName);

            var weathers = Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();

            // Adds an event to the trace with some tags
            activity?.AddEvent(new ActivityEvent("Load Weather",
                tags: new ActivityTagsCollection(new[] { KeyValuePair.Create<string, object?>("Count", weathers.Count()) })));
            activity?.AddTag("weather.count", weathers.Count());
            activity?.AddTag("otel.status_code", "OK");
            activity?.AddTag("otel.status_description", "Load Successfully");

            // Adds Metrics
            DiagnosticConfiguration.AddWeatherMetrics(weathers.Count(), weathers.First().Summary!);

            // Adds more tags to trace using the current activity object which is the same object as above.
            // It is used when you not in the same scope as where the activity object was created.
            Activity.Current?.EnrichWithName("Jarryd");

            return weathers;
        }

        [HttpGet("exception", Name = "Exception")]
        public IActionResult ThrowException()
        {
            try
            {
                DiagnosticConfiguration.ActivitySource.StartActivity(DiagnosticConfiguration.SourceName);
                throw new Exception("Testing tracing");
            }
            catch (Exception ex)
            {
                Activity.Current?.SetStatus(ActivityStatusCode.Error);
                Activity.Current?.RecordException(ex, new TagList()
                {
                    { "my_name", "Jarryd" }
                });
                throw;
            }

            return Ok();
        }
    }
}
