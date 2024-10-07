using MesDataCollection.Job;
using MesDataCollection.Repository;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using Newtonsoft.Json;
using System.Net.NetworkInformation;

namespace MesDataCollection
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            try
            {
                IConfigurationBuilder builder = new ConfigurationBuilder()
                   .SetBasePath(Directory.GetCurrentDirectory())
                   .AddJsonFile("appsettings.json");

                Configuration = builder.Build();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"运行异常：{ex.Message},{ex.ToString()}");
            }
        }

        public IConfiguration Configuration { get; }
        
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers().AddNewtonsoftJson();
            services.AddScoped<DataRepository>();
            services.AddScoped<UserRepository>();
            services.AddHostedService<JobService>();

            services.AddCors(options => options.AddPolicy("CorsPolicy",
                 builder =>
                 {
                     builder.AllowAnyMethod()
                         .SetIsOriginAllowed(_ => true)
                         .AllowAnyHeader()
                         .AllowCredentials();
                 }));
        }

      
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

            
        }
    }
}
