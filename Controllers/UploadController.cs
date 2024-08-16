using MesDataCollection.Model;
using MesDataCollection.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Spectre.Console;
using System;
using System.IO;
using System.Threading.Tasks;

namespace MesDataCollection.Controllers
{

    [ApiController]
    public class UploadController : ControllerBase
    {
        private readonly DataRepository _databaseService;
        private readonly ILogger<UploadController> _logger;
        public static IConfigurationRoot Configuration { get; set; }

        public UploadController(DataRepository databaseService, ILogger<UploadController> logger)
        {
            _databaseService = databaseService;
            _logger = logger;
            IConfigurationBuilder builder = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json");

            Configuration = builder.Build();
        }

        [HttpPost]
        [Route("api/[controller]")]
        public async Task<IActionResult> Post([FromBody] UploadModel model)
        {
            try
            {
                LogMessage("INFO", $"接收数据：{JsonConvert.SerializeObject(model)}");
                if (model == null)
                {
                    return BadRequest("No data provided.");
                }

                // 检查表是否存在，如果不存在则创建表
                if (!await _databaseService.TableExistsAsync())
                {
                    await _databaseService.CreateTableAsync();
                }
                if (model.TestResult != null && model.TestResult.Count > 0)
                {
                    foreach (var result in model.TestResult)
                    {
                        await _databaseService.SaveUploadModelAsync(model, result);
                    }
                }
                return Ok("saved successfully.");
            }
            catch (Exception ex)
            {
                LogMessage("ERRO", ex.Message);
                return StatusCode(500, "An error occurred while saving data.");
            }
        }


     
        public static void LogMessage(string level, string message)
        {
            var now = DateTime.Now.ToString("HH:mm:ss");
            AnsiConsole.WriteLine($"[{now}] [{level}] {message}");
        }
    }
}
