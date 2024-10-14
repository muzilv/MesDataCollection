using MesDataCollection.Repository;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using System;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MesDataCollection.Job
{
    public class JobService : BackgroundService
    {
        private readonly ILogger<JobService> _logger;
        DataRepository _databaseService=new DataRepository ();
        string[] testResultsToUpdate = new[] { "键帽不良", "胶路不良", "塑胶件不良", "CCD不良", "冷冻不良" };

        string[] ProcessNameList = new[] { "点胶机", "按键贴合", "成品检测" };
        public JobService(ILogger<JobService> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await SumPlan();
                await SumUploadData();
                await Task.Delay(1000 * 20, stoppingToken);
            }
        }


        private async Task SumPlan()
        {
            try
            {
                var palns = await _databaseService.GetMesPlan();
                if (palns.Any())
                {
                    foreach (var pal in palns)
                    {
                        var dataqty = await _databaseService.GetUploadDataQty(pal.Start_Time, pal.End_Time);
                        if (dataqty != null && dataqty.qty > 0 && dataqty.qty != pal.ActualQty)
                        {
                            pal.ActualQty = dataqty.qty;
                            await _databaseService.UpdateMesPlan(pal);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"任务异常：{ex.Message}");
            }
        }


        private async Task SumUploadData()
        {
            try
            {
                DateTime now = DateTime.Now;
                if (!await _databaseService.GetSumData(now.Date))
                {
                    await _databaseService.CreateSumData(now.Date);
                };
                if (!await _databaseService.GetSumData(now.Date, "Line1"))
                {
                    await _databaseService.CreateSumData(now.Date, "Line1");
                };
                if (!await _databaseService.GetSumData(now.Date, "Line2"))
                {
                    await _databaseService.CreateSumData(now.Date, "Line2");
                };
                await SumHourDefectiveFraction();
                await SumHourDefectiveFraction("Line1");
                await SumHourDefectiveFraction("Line2");
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"任务异常：{ex.Message}");
            }
        }



        //ProcessName=点胶机/按键贴合/成品检测(固定值)
        //投入数=成品总数+键帽不良数+胶路不良数
        //成品产出=成品检测上传得良品数
        //不良数/不良率=投入数-成品产出/(投入数-成品产出)/投入数100
        //24小时不良率统计就计算每种不良数量/每个流程上传的总数

        /// <summary>
        /// 更新每小时数据
        /// </summary>
        /// <param name="now"></param>
        /// <param name="ResultQty"></param>
        /// <returns></returns>
        private async Task SumHourDefectiveFraction(string LineName="")
        {
            DateTime now = DateTime.Now;
            DateTime start = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);
            DateTime end = new DateTime(now.Year, now.Month, now.Day, 23, 59, 59);

            var ProcessQtyList = await _databaseService.GetProcessQty(start, end, LineName);
            if (ProcessQtyList != null && ProcessQtyList.Count() > 0)
            {
                var hourList = ProcessQtyList.Select(x => x.hour).Distinct().ToArray();
                foreach (var hour in hourList)
                {
                    long finishedQty = ProcessQtyList.Where(x => x.hour == hour&&x.ProcessName== "成品检测"&&x.TestResult== "成品产出").Sum(x => x.qty);
                    long inputQty = ProcessQtyList.Where(x => x.hour == hour && x.TestResult == "键帽不良" || x.TestResult == "胶路不良").Sum(x => x.qty)+ finishedQty;
                    await _databaseService.UpdateQtys(inputQty.ToString(), hour, "投入数", now.Date, LineName);
                    await _databaseService.UpdateQtys(finishedQty.ToString(), hour, "成品产出", now.Date, LineName);

                    foreach (var testResult in testResultsToUpdate)
                    {
                        long defectsQty = ProcessQtyList.Where(x => x.TestResult == testResult).Sum(x => x.qty);
                        await _databaseService.UpdateQtys(defectsQty.ToString(), hour, testResult, now.Date, LineName);

                        var percentage =Convert.ToDouble((defectsQty / Convert.ToDecimal(inputQty)) * 100).ToString("F2") + "%";
                        await _databaseService.UpdateQtys(percentage, hour, $"{testResult}率%", now.Date, LineName);
                    }
                }

                //更新合计数据
                {
                    long finishedQty = ProcessQtyList.Where(x => x.ProcessName == "成品检测" && x.TestResult == "成品产出").Sum(x => x.qty);
                    long inputQty = ProcessQtyList.Where(x =>  x.TestResult == "键帽不良" || x.TestResult == "胶路不良").Sum(x => x.qty) + finishedQty;
                    await _databaseService.UpdateQtys(inputQty.ToString(), "投入数", now.Date, LineName);
                    await _databaseService.UpdateQtys(finishedQty.ToString(), "成品产出", now.Date, LineName);

                    foreach (var testResult in testResultsToUpdate)
                    {
                        long defectsQty = ProcessQtyList.Where(x => x.TestResult == testResult).Sum(x => x.qty);
                        await _databaseService.UpdateQtys(defectsQty.ToString(), testResult, now.Date, LineName);

                        var percentage = Convert.ToDouble((defectsQty / Convert.ToDecimal(inputQty)) * 100).ToString("F2") + "%";
                        await _databaseService.UpdateQtys(percentage, $"{testResult}率%", now.Date, LineName);
                    }
                }
            }
        }


        

    }
}
