using System;
using System.IO;
using Intreba.Identity.Api.Config;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace Intreba.Identity.Api
{
    public class Program
    {
        public static IConfiguration Configuration { get; set; }
        public static void Main(string[] args)
        {
            Configuration = CreateConfiguration();

            Logging.Config(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development");

            try
            {
                Log.Information("Starting up application");

                var host = BuildWebHost(args);

                host.Run();

                //return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
                //return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static IConfiguration CreateConfiguration()
        {
            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseConfiguration(CreateConfiguration())
                .UseSerilog()
                .UseStartup<Startup>()
                .Build();
    }
}
