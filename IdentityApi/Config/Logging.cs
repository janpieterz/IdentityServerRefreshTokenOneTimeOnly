using Serilog;
using Serilog.Events;

namespace Intreba.Identity.Api.Config
{
    public class Logging

    {
        public static void Config(bool isDevelopment)
        {
            var loggerConfiguration = new LoggerConfiguration();
            if (!isDevelopment)
            {
                loggerConfiguration
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                    .MinimumLevel.Override("System", LogEventLevel.Warning)
                    .MinimumLevel.Override("NServiceBus", LogEventLevel.Warning);
            }
            Log.Logger = loggerConfiguration
                .Enrich.FromLogContext()
                .Enrich.WithProperty("EndpointName", "Api")
                .Enrich.WithMachineName()
                .Enrich.FromLogContext()
                .WriteTo.RollingFile("logs/{Date}.txt", LogEventLevel.Debug, "{Timestamp:yyyy-MM-dd HH:mm:ss} ({SourceContext}) [{Level}] {Message}{NewLine}{Exception}")
                .WriteTo.LiterateConsole(LogEventLevel.Debug, "{Timestamp:yyyy-MM-dd HH:mm:ss} ({SourceContext}) [{Level}] {Message}{NewLine}{Exception}")
                .MinimumLevel.Debug()
                .CreateLogger();
        }
    }
} 