using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using WebScrapingService;
using WebScrapingService.Options;
using WebScrapingService.Twilio;

namespace HostedService
{
    class Program
    {
        static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .UseWindowsService()
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddSerilog();
                })
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddJsonFile("appsettings.json");
                })
                .UseSerilog((hostingContext, services, loggerConfiguration) =>
                {
                    loggerConfiguration.MinimumLevel.Debug();
                    loggerConfiguration.Enrich.FromLogContext();
                    loggerConfiguration.WriteTo.ColoredConsole();
                    loggerConfiguration.WriteTo.File("logs\\log.txt", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 5);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<MicroCenterStockChecker>();
                    services.Configure<MicroCenterOptions>(hostContext.Configuration.GetSection(MicroCenterOptions.SectionKey));
                    services.Configure<TwilioOptions>(hostContext.Configuration.GetSection(TwilioOptions.SectionKey));
                    services.AddScoped<ITwilioService, TwilioService>();
                });

            return host;
        }
    }
}
