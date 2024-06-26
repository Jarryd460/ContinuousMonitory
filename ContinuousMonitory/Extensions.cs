﻿using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;

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
        // Adds Serilog as my log provider and configure to log to File (Promtail uses the logs to read and then push to Loki), Console, Loki
        // (Serilog pushes the logs directly as well) and OpenTelemetry Collector (see README.md to see how to get it running).
        // See AppSettings.Development.json. Also see how Loki and Promtail is setup.
        builder.Services.AddSerilog(configure => 
        {
            configure.ReadFrom.Configuration(builder.Configuration);
        });

        // The built-in logging can also send telemetry data but requires Serilog to be removed/commented out
        //builder.Logging.AddOpenTelemetry(options => {
        //    options.IncludeFormattedMessage = true;
        //    options.IncludeScopes = true;
        //    options
        //        .AddConsoleExporter()
        //        .AddOtlpExporter(options =>
        //        {
        //            options.Endpoint = new Uri("http://localhost:4318/v1/logs");
        //            options.Protocol = OtlpExportProtocol.HttpProtobuf;
        //        });
        //});

        // Captures metrics and traces in OpenTelemetry format to be exported
        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource =>
            {
                resource
                    .AddService(
                        serviceName: Environment.GetEnvironmentVariable("SERVICE_NAME") ?? builder.Environment.ApplicationName,
                        serviceVersion: System.Reflection.Assembly.GetEntryAssembly()?.GetName().Version?.ToString(3) // SemVer
                    )
                    .AddAttributes(new Dictionary<string, object>
                    {
                        { "host.name", Environment.MachineName }
                    });
            })
            .WithMetrics(configure =>
            {
                configure
                    .AddRuntimeInstrumentation()
                    .AddMeter(
                        "Microsoft.AspNetCore.Hosting",
                        "Microsoft.AspNetCore.Server.Kestrel",
                        "System.Net.Http",
                        DiagnosticConfiguration.Meter.Name)
                    .AddAspNetCoreInstrumentation()
                    .AddProcessInstrumentation()
                    .AddHttpClientInstrumentation()
                    // Uncomment this code in order for metrics to made available in a format for Prometheus via this Api
                    //.AddPrometheusExporter(configure =>
                    //{
                    //    // Added so that we can add instruments for Runtime but it still fails after a while
                    //    configure.DisableTotalNameSuffixForCounters = true;
                    //})
                    .AddConsoleExporter()
                    .AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri("http://localhost:4318/v1/metrics");
                        options.Protocol = OtlpExportProtocol.HttpProtobuf;
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
                        options.Filter = (request) => !request.Request.Path.ToUriComponent().Contains("index.html", StringComparison.OrdinalIgnoreCase) &&
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
                    .AddConsoleExporter()
                    .AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri("http://localhost:4318/v1/traces");
                        options.Protocol = OtlpExportProtocol.HttpProtobuf;
                    });
            });

        return builder;
    }

    public static WebApplication MapDefaultEndpoints(this WebApplication app)
    {
        // Uncomment this line to expose a /metrics url so Prometheus can scrap the metrics formatted by .AddPrometheusExporter
        //app.MapPrometheusScrapingEndpoint();

        return app;
    }
}
