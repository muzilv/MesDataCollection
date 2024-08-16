using MesDataCollection.Job;
using MesDataCollection.Repository;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace MesDataCollection
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            IConfigurationBuilder builder = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json");

            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddScoped<DataRepository>();
            services.AddHostedService<JobService>();

            //允许所有跨域
            services.AddCors(options => options.AddPolicy("CorsPolicy",
                 builder =>
                 {
                     builder.AllowAnyMethod()
                         .SetIsOriginAllowed(_ => true)
                         .AllowAnyHeader()
                         .AllowCredentials();
                 }));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthorization();
           
            //允许所有跨域
            app.UseCors("CorsPolicy");

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            string linkString = Configuration.GetConnectionString("DefaultConnection");

            int startIndex = linkString.IndexOf("Server=") + "Server=".Length;
            int endIndex = linkString.IndexOf(";", startIndex);
            string ipAddress = linkString.Substring(startIndex, endIndex - startIndex).Trim();

            var ping = new Ping();
            var reply = ping.SendPingAsync(ipAddress).Result;
            if (reply.Status != IPStatus.Success)
            {
                Console.WriteLine($"数据量连接失败，IP address {ipAddress} is not reachable.");
            }

            // 添加异常处理中间件
            app.UseExceptionHandler(c => c.Run(async context =>
            {
                var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
                if (exception != null)
                {
                    // 在这里处理异常
                    if (exception is AggregateException aggregateException)
                    {
                        foreach (var innerException in aggregateException.InnerExceptions)
                        {
                            Console.WriteLine($"An error occurred: {innerException.Message}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"An error occurred: {exception.Message}");
                    }
                    await context.Response.WriteAsync($"An error occurred: {exception.Message}");
                }
            }));
        }
    }
}
