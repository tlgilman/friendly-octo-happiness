using JA.Logging.Extensions.Serilog;
using JA.Logging.Extensions.Serilog.Web;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;

namespace TravelGuideApi;

public class Program
{
    public static void Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .ConfigureDefaultSettings()
            .WriteToConsole(UseStructuredLogging())
            .CreateLogger();

        try
        {
            CreateHostBuilder(args).Build().Run();
        }
        catch (Exception ex)
        {
            Log.Logger.Error(ex, "An unhandled error occurred. See exception details.");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    private static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            })
            .UseSerilog((context, services, configuration) =>
            {
                configuration.ConfigureDefaultSettings()
                    .AddClientEnrichers()
                    .FilterInformationLogs(services)
                    .WriteToConsole(UseStructuredLogging());
            });
    }

    private static bool UseStructuredLogging()
    {
        bool.TryParse(
            Environment.GetEnvironmentVariable(
                "USE_PLAIN_LOGGING",
                EnvironmentVariableTarget.Process),
            out bool usePlainLogging);

        return !usePlainLogging;
    }
}
