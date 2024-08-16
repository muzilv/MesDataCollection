using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using System;
using System.Threading.Tasks;

namespace MesDataCollection
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            try
            {
                AnsiConsole.Write(new FigletText("MesDataCollectionServer").Centered().Color(Color.Blue));
                AnsiConsole.Console.WriteLine($"服务启动中{DateTime.Now}");

                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                AnsiConsole.Console.WriteLine($"服务异常{DateTime.Now},{ex.Message}");
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>().UseUrls("http://*:5000;https://*:5001"); ;
                }).ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                });

    }
}
