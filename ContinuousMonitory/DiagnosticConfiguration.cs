using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace ContinuousMonitory;

// You should create one of these per project or assembly
internal static class DiagnosticConfiguration
{
    public const string SourceName = "ContinuousMonitory";
    public static Meter Meter = new Meter(SourceName);
    public static Counter<int> WeatherCount = Meter.CreateCounter<int>("weather.count");
    public static Histogram<int> WeatherHistogram = Meter.CreateHistogram<int>("weather.histogram.count");
    public static ActivitySource ActivitySource = new ActivitySource(SourceName);

    public static void AddWeatherMetrics(int weatherCount, string summary)
    {
        // Do not add to many labels as it grows exponentially
        var labels = new KeyValuePair<string, object?>(DiagnosticNames.Summary, summary);

        WeatherHistogram.Record(weatherCount, labels);
        WeatherCount.Add(1, labels);
    }
}
