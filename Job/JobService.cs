using MesDataCollection.Model;
using MesDataCollection.Repository;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using System;
using System.Collections.Generic;
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
                await Task.Delay(1000 * 60, stoppingToken);
            }
        }


        private async Task SumPlan()
        {
            try
            {
                var palns = await _databaseService.GetMesPlan(DateTime.Now);
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
                DateTime start = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);
                DateTime end = new DateTime(now.Year, now.Month, now.Day, 23, 59, 59);

                if (!await _databaseService.GetSumData(now.Date))
                {
                    await _databaseService.CreateSumData(now.Date);
                };
                var ResultQty = await _databaseService.GetTestResultQty(start, end);
                if (ResultQty != null && ResultQty.Count > 0)
                {
                    await sumPass();

                    await SumHourDefectiveFraction(now, ResultQty);

                    await sunHourQty(now, ResultQty);
                    await sunHourRate(now, ResultQty);
                }
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
        /// 更新每小时各流程数量
        /// </summary>
        /// <param name="now"></param>
        /// <param name="ResultQty"></param>
        /// <returns></returns>
        private async Task sumPass()
        {
            DateTime now = DateTime.Now;
            DateTime start = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);
            DateTime end = new DateTime(now.Year, now.Month, now.Day, 23, 59, 59);

            var ResultQty = await _databaseService.GetTestResultQty(start, end);
            if (ResultQty != null && ResultQty.Count > 0)
            {
                ResultQty.RemoveAll(x => x.TestResult == "成品产出" || x.TestResult.ToUpper() == "CCD不良");
                foreach (var item in ResultQty)
                {
                    await _databaseService.UpdateQty(item, now.Date);
                }
            }

            ResultQty = await _databaseService.GetProductQty(start, end);
            if (ResultQty != null && ResultQty.Count > 0)
            {
                foreach (var item in ResultQty)
                {
                    await _databaseService.UpdateQty(item, now.Date);
                }
            }
        }


        /// <summary>
        /// 更新每小时不良率
        /// </summary>
        /// <param name="now"></param>
        /// <param name="ResultQty"></param>
        /// <returns></returns>
        private async Task SumHourDefectiveFraction(DateTime now, List<UploadStatus> ResultQty)
        {
            var testResultsToUpdate = new[] { "键帽不良", "胶路不良", "塑胶件不良", "成品产出", "CC不良", "冷冻不良" };
            var hourList = ResultQty.Select(x => x.hour).Distinct().ToArray();
            if (hourList.Length > 0)
            {
                foreach (var hour in hourList)
                {
                    var sumQty = ResultQty.Where(x => x.hour == hour).Sum(x => x.qty);
                    await _databaseService.UpdateQtys(sumQty.ToString(), hour, "投入数", now.Date);

                    var resultMap = new Dictionary<string, long?>();

                    foreach (var testResult in testResultsToUpdate)
                    {
                        var qty = ResultQty.Where(x => x.TestResult == testResult)?.Sum(x => x.qty);
                        resultMap[testResult] = qty;
                    }

                    foreach (var entry in resultMap)
                    {
                        var qty = entry.Value;
                        await _databaseService.UpdateQtys(qty.ToString(), hour, $"{entry.Key}", now.Date);
                    }
                    foreach (var entry in resultMap)
                    {
                        var qty = entry.Value;
                        var percentage = qty.HasValue ? Convert.ToDouble((qty.Value / Convert.ToDecimal(sumQty)) * 100).ToString("F2") + "%" : "0%";
                        await _databaseService.UpdateQtys(percentage, hour, $"{entry.Key}不良率%", now.Date);
                    }
                }
            }
        }

        /// <summary>
        /// 统计天维度数量
        /// </summary>
        /// <param name="now"></param>
        /// <param name="ResultQty"></param>
        /// <returns></returns>
        private async Task sunHourQty(DateTime now, List<UploadStatus> ResultQty)
        {
            var sumQty = ResultQty.Sum(x => x.qty);
            await _databaseService.UpdateQtys(sumQty.ToString(), "投入数", now.Date);

            var testResultsToUpdate = new[] { "键帽不良", "胶路不良", "塑胶件不良", "成品产出", "CC不良", "冷冻不良"};

            var resultMap = new Dictionary<string, long?>();

            foreach (var testResult in testResultsToUpdate)
            {
                var qty = ResultQty.Where(x => x.TestResult == testResult)?.Sum(x => x.qty);
                resultMap[testResult] = qty;
            }

            // 并行更新数据库
            await Task.WhenAll(
                testResultsToUpdate.Select(async testResult =>
                {
                    if (resultMap.TryGetValue(testResult, out long? qty) && qty.HasValue)
                    {
                        await _databaseService.UpdateQtys(qty.Value.ToString(), testResult, now.Date);
                    }
                })
            );
        }

        /// <summary>
        /// 统计天维度不良率
        /// </summary>
        /// <param name="now"></param>
        /// <param name="ResultQty"></param>
        /// <returns></returns>
        private async Task sunHourRate(DateTime now, List<UploadStatus> ResultQty)
        {
            // 定义常量集合
            var testResultsToUpdate = new Dictionary<string, string>
            {
               { "键帽不良", "键帽不良率%" },
               { "胶路不良", "胶路不良率%" },
               { "塑胶件不良", "塑胶件不良率%" },
               { "冷冻不良", "冷冻不良率%" }
            };

            // 计算总数
            var sumhourQty = Convert.ToDecimal(ResultQty.Sum(x => x.qty));

            // 构建字典并计算百分比
            var resultMap = new Dictionary<string, string>();

            foreach (var entry in testResultsToUpdate)
            {
                var qty = ResultQty.Where(x => x.TestResult == entry.Key)?.Sum(x => x.qty);
                var percentage = qty.HasValue ? Convert.ToDouble((qty.Value / sumhourQty) * 100).ToString("F2") + "%" : "0%";
                resultMap[entry.Value] = percentage;
            }

            // 并行更新数据库
            await Task.WhenAll(
                resultMap.Select(async pair =>
                {
                    await _databaseService.UpdateQtys(pair.Value, pair.Key, now.Date);
                })
            );
        }
    }
}
