using MesDataCollection.Model;
using MesDataCollection.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace MesDataCollection.Controllers
{
    [ApiController]
    public class DadaController : ControllerBase
    {

        private readonly DataRepository _databaseService;
        private readonly ILogger<UploadController> _logger;
        public static IConfigurationRoot Configuration { get; set; }
        string[] testResultsToUpdate = new[] { "键帽不良", "胶路不良", "塑胶件不良", "CCD不良", "冷冻不良" };

        public DadaController(DataRepository databaseService, ILogger<UploadController> logger)
        {
            _databaseService = databaseService;
            _logger = logger;
            IConfigurationBuilder builder = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json");

            Configuration = builder.Build();
        }

        [HttpPost]
        [Route("api/[controller]/[action]")]
        public async Task<IActionResult> CreateProductionPlan([FromBody] ProductionPlan model)
        {
            LogMessage("INFO", $"接收数据：{JsonConvert.SerializeObject(model)}");
            if (model == null)
            {
                return BadRequest("No data provided.");
            }

            try
            {
                if (model != null)
                {
                    await _databaseService.CreateProductionPlan(model);
                }
                return Ok("saved successfully.");
            }
            catch (Exception ex)
            {
                LogMessage("ERRO", ex.Message);
                return StatusCode(500, "An error occurred while saving data.");
            }
        }

        [HttpGet]
        [Route("api/[controller]/[action]")]
        public async Task<IActionResult> GetMesPlan()
        {
            try
            {
                var palns = await _databaseService.GetMesPlans(DateTime.Now);
                return Ok(palns);
            }
            catch (Exception ex)
            {
                LogMessage("ERRO", ex.Message);
                return StatusCode(500, "An error occurred while saving data.");
            }
        }

        [HttpGet]
        [Route("api/[controller]/[action]")]
        public async Task<IActionResult> DeleteMesPlan(string id)
        {
            try
            {
                var isTrue = await _databaseService.DeleteMesPlans(id);
                if (isTrue)
                {
                    return Ok(isTrue);
                }
                return StatusCode(400, "An error occurred while saving data.");
            }
            catch (Exception ex)
            {
                LogMessage("ERRO", ex.Message);
                return StatusCode(500, "An error occurred while saving data.");
            }
        }

        [HttpGet]
        [Route("api/[controller]/[action]")]
        public async Task<IActionResult> GetMesSumData()
        {
            try
            {
                List<string[]> strings = new List<string[]>();
                var data = await _databaseService.GetMesSumData();
                string json = JsonConvert.SerializeObject(data);
                foreach (var item in data)
                {
                    // 将实体类的字段值转换为字符串数组
                    string[] fieldValues = ConvertEntityFieldsToStringArray(item);
                    strings.Add(fieldValues);

                }

                return Ok(strings);

            }
            catch (Exception ex)
            {
                LogMessage("ERRO", ex.Message);
                return StatusCode(500, "An error occurred while saving data.");
            }
        }

        [HttpGet]
        [Route("api/[controller]/[action]")]
        public async Task<IActionResult> GetUploadResult()
        {
            try
            {
                DateTime now = DateTime.Now;
                DateTime start = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);
                DateTime end = new DateTime(now.Year, now.Month, now.Day, 23, 59, 59);
                List<CapsuleChart> list = new List<CapsuleChart>();
                var ProcessQtyList = await _databaseService.GetProcessQty(start, end,"");
                if (ProcessQtyList != null && ProcessQtyList.Count() > 0)
                {

                    long finishedQty = ProcessQtyList.Where(x => x.ProcessName == "成品检测" && x.TestResult == "成品产出").Sum(x => x.qty);
                    list.Add(new CapsuleChart { name = "成品产出", value = finishedQty });
                    foreach (var testResult in testResultsToUpdate)
                    {
                        long defectsQty = ProcessQtyList.Where(x => x.TestResult == testResult).Sum(x => x.qty);
                        list.Add(new CapsuleChart { name = testResult, value = defectsQty });
                    }
                }
               
                return Ok(list);
            }
            catch (Exception ex)
            {
                LogMessage("ERRO", ex.Message);
                return StatusCode(500, "An error occurred while saving data.");
            }
        }

        [HttpGet]
        [Route("api/[controller]/[action]")]
        public async Task<IActionResult> GetPlanQty()
        {
            try
            {
                DateTime now = DateTime.Now;
                DateTime start = new DateTime(now.Year, now.Month, now.Day).AddDays(-15);
                DateTime end = new DateTime(now.Year, now.Month, now.Day);
                var data = await _databaseService.GetMesPlan(start, end);

                List<string> datelist = new List<string>();
                List<long> Planlist = new List<long>();
                List<long> Actuallist = new List<long>();
                for (DateTime i = start; i <= end; i = i.AddDays(1))
                {
                    datelist.Add(i.ToString("yyyy-MM-dd"));

                    Planlist.Add(data?.FirstOrDefault(x => x.start_time == i)?.plan_quantity ?? 0);
                    Actuallist.Add(data?.FirstOrDefault(x => x.start_time == i)?.ActualQty ?? 0);
                }


                return Ok(new { date = datelist, Planlist = Planlist, Actuallist = Actuallist });
            }
            catch (Exception ex)
            {
                LogMessage("ERRO", ex.Message);
                return StatusCode(500, "An error occurred while saving data.");
            }
        }

        [HttpGet]
        [Route("api/[controller]/[action]")]
        public async Task<IActionResult> GetOutputStatistics()
        {
            try
            {
                DateTime now = DateTime.Now;
                DateTime start = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);
                DateTime end = new DateTime(now.Year, now.Month, now.Day, 23, 59, 59);
                var data = await _databaseService.GetOutputStatistics(start, end);
                var plan = await _databaseService.GetMesPlanDefault(start, end);
                if (data != null)
                {

                    data.FialQty = data.TotalQty - data.PassQty;
                    data.ForecastQty = Convert.ToInt32((Convert.ToDecimal(data.PassQty) / now.Hour) * 24);
                    data.PlannedQty = Convert.ToInt32(plan?.Plan_Quantity ?? "0");
                    string[] fieldValues = ConvertEntityFieldsToStringArray(data);
                    return Ok(fieldValues);
                }
                return Ok();
            }
            catch (Exception ex)
            {
                LogMessage("ERRO", ex.Message);
                return StatusCode(500, "An error occurred while saving data.");
            }
        }

        [HttpGet]
        [Route("api/[controller]/[action]")]
        public async Task<IActionResult> GetMesPlanDefault()
        {
            try
            {
                DateTime now = DateTime.Now;
                DateTime start = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);
                DateTime end = new DateTime(now.Year, now.Month, now.Day, 23, 59, 59);

                var data = await _databaseService.GetMesPlanDefault(start, end);
                if (data != null)
                {
                    return Ok(data);
                }
                return Ok();
            }
            catch (Exception ex)
            {
                LogMessage("ERRO", ex.Message);
                return StatusCode(500, "An error occurred while saving data.");
            }
        }

        [HttpGet]
        [Route("api/[controller]/[action]")]
        public async Task<IActionResult> GetDayQty()
        {
            try
            {
                DateTime now = DateTime.Now;
                DateTime start = new DateTime(now.Year, now.Month, now.Day).AddDays(-15);
                DateTime end = new DateTime(now.Year, now.Month, now.Day);
                var data = await _databaseService.GetDayQty();
                if (data != null)
                {
                    string[] fieldValues = ConvertEntityFieldsToStringArray(data);
                    return Ok(fieldValues);
                }
                return Ok();
            }
            catch (Exception ex)
            {
                LogMessage("ERRO", ex.Message);
                return StatusCode(500, "An error occurred while saving data.");
            }
        }

        [HttpGet]
        [Route("api/[controller]/[action]")]
        public async Task<IActionResult> GetLine()
        {
            try
            {
                DateTime now = DateTime.Now;
                DateTime start = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);
                DateTime end = new DateTime(now.Year, now.Month, now.Day, 23, 59, 59);
                var data = await _databaseService.GetLineModel(start, end);
                return Ok(data);
            }
            catch (Exception ex)
            {
                LogMessage("ERRO", ex.Message);
                return StatusCode(500, "An error occurred while saving data.");
            }
        }


        private static string[] ConvertEntityFieldsToStringArray(object obj)
        {
            Type type = obj.GetType();
            PropertyInfo[] properties = type.GetProperties();
            string[] stringArray = new string[properties.Length];

            for (int i = 0; i < properties.Length; i++)
            {
                stringArray[i] = properties[i].GetValue(obj)?.ToString();
            }

            return stringArray;
        }

        public static void LogMessage(string level, string message)
        {
            var now = DateTime.Now.ToString("HH:mm:ss");
            Console.WriteLine($"[{now}] [{level}] {message}");
        }
    }
}
