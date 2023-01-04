using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Sinks.MSSqlServer;
using System;

namespace ProjetoBanco
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json").Build();

            var columnOpts = new ColumnOptions();
            columnOpts.Store.Remove(StandardColumn.Properties);

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(config)
                .WriteTo.MSSqlServer(
                    config.GetConnectionString("ProjetoBancoContext"),
                    sinkOptions: new MSSqlServerSinkOptions
                    {
                        AutoCreateSqlTable = true,
                        TableName = "Logs",
                    }
                )

                .WriteTo.File(path: "C:\\Trilobit\\Logs\\ProjetoBanco\\log-.txt", rollingInterval: RollingInterval.Day
                , outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            try
            {
                Log.Information("API Inicianda.");
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "A API falhou ao iniciar.");
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
