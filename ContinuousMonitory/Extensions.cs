using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Runtime.CompilerServices;

namespace ContinuousMonitory;

internal static class Extensions
{
    public static IHostApplicationBuilder AddServiceDefaults(this IHostApplicationBuilder builder)
    {
        builder.ConfigureOpenTelemetry();

        return builder;
    }

    public static IHostApplicationBuilder ConfigureOpenTelemetry(this IHostApplicationBuilder builder)
    {
        builder.Logging.AddOpenTelemetry(configure => {
            configure.IncludeScopes = true;
            configure.IncludeFormattedMessage = true; 
        });

        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(
                serviceName: Environment.GetEnvironmentVariable("SERVICE_NAME") ?? builder.Environment.ApplicationName,
                serviceVersion: System.Reflection.Assembly.GetEntryAssembly()?.GetName().Version?.ToString(3) // SemVer
            )
            .AddAttributes(new Dictionary<string, object>
                {
                    { "host.name", Environment.MachineName }
                })
            )
            .WithMetrics(configure =>
            {
                configure
                    //.AddRuntimeInstrumentation()
                    .AddMeter(
                        "Microsoft.AspNetCore.Hosting",
                        "Microsoft.AspNetCore.Server.Kestrel",
                        "System.Net.Http",
                        DiagnosticConfiguration.Meter.Name)
                    .AddAspNetCoreInstrumentation()
                    .AddProcessInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddPrometheusExporter(configure =>
                    {
                        // Added so that we can add instruments for Runtime but it still fails after a while
                        configure.DisableTotalNameSuffixForCounters = true;
                    });
            })
            .WithTracing(configure =>
            {
                if (builder.Environment.IsDevelopment())
                {
                    configure.SetSampler<AlwaysOnSampler>();
                }
                configure
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        options.Filter = (request) => !request.Request.Path.ToUriComponent().Contains("index.html", StringComparison.OrdinalIgnoreCase) ||
                            !request.Request.Path.ToUriComponent().Contains("swagger", StringComparison.OrdinalIgnoreCase);
                    })
                    .AddSqlClientInstrumentation(options =>
                    {
                        options.SetDbStatementForText = true;
                        options.RecordException = true;
                    })
                    .AddEntityFrameworkCoreInstrumentation(configure =>
                    {
                        configure.SetDbStatementForText = true;
                    })
                    .AddHttpClientInstrumentation()
                    .AddSource(DiagnosticConfiguration.SourceName)
                    .AddConsoleExporter();
            });

        builder.AddOpenTelemetryExporters();

        return builder;
    }

    public static IHostApplicationBuilder AddOpenTelemetryExporters(this IHostApplicationBuilder builder)
    {
        var useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

        if (useOtlpExporter)
        {
            builder.Services.Configure<OpenTelemetryLoggerOptions>(logger => logger.AddOtlpExporter());
            builder.Services.ConfigureOpenTelemetryMeterProvider(metrics => metrics.AddOtlpExporter());
            builder.Services.ConfigureOpenTelemetryTracerProvider(tracer => tracer.AddOtlpExporter());
        }

        return builder;
    }

    public static WebApplication MapDefaultEndpoints(this WebApplication app)
    {
        app.MapPrometheusScrapingEndpoint();

        return app;
    }
}
