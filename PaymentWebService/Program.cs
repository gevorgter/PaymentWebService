using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace PaymentWebService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
           //.Enrich.FromLogContext()
           .Enrich.WithProperty("App", "PaymentWebService")
           .MinimumLevel.Information()
           .MinimumLevel.Override("Microsoft", LogEventLevel.Error)
           //.WriteTo.File(new RenderedCompactJsonFormatter(), "log.txt", LogEventLevel.Information)//, outputTemplate: "[{Timestamp:HH:mm:ss}{Level:u3}] {Message:lj}{NewLine}{Exception}")
           .WriteTo.File("log.txt", LogEventLevel.Information, outputTemplate: "[{Timestamp:HH:mm:ss}{Level:u3}] {Message:lj}{Properties:j}{NewLine}{Exception}")
           .CreateLogger();

            try
            {
                Log.Information("Starting up");
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application start-up failed");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
