using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting; 
using Microsoft.Extensions.Hosting; 
using Serilog;

namespace CoreWeb.Consumer.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                                  .MinimumLevel.Debug()
                                  .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
                                  .Enrich.WithProperty("AppName", "HR.BulkLeaveOperation")
                                  .Enrich.FromLogContext()
                                  .WriteTo.File($"C:/temp/logs/{DateTime.UtcNow:yyyy-MM-dd}.log")
                                  .CreateLogger();

            try
            {
                Log.Information("Consumer is started");
                CreateHostBuilder(args).Build().Run();
                return;
            }
            catch (Exception ex)
            {
                Log.Fatal("There was a problem starting the consumer service");
                Log.Fatal(ex, "CreateHostBuilder");
                return;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
