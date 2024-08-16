using MesDataCollection.Repository;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
                    foreach (var item in ResultQty)
                    {
                        if (item.TestResult == "Pass")
                        {
                            item.TestResult = "成品产出";
                        }
                        await _databaseService.UpdateQty(item, now.Date);
                    }
                    var hourList = ResultQty.Select(x => x.hour).Distinct().ToArray();
                    if (hourList.Length > 0)
                    {
                        foreach (var hour in hourList)
                        {
                            var sumQty = ResultQty.Where(x => x.hour == hour).Sum(x => x.qty);
                            await _databaseService.UpdateQtys(sumQty.ToString(), hour, "投入数", now.Date);

                            var jmblQty = ResultQty.Where(x => x.hour == hour).FirstOrDefault(x => x.TestResult == "键帽不良")?.qty;
                            var jlblQty = ResultQty.Where(x => x.hour == hour).FirstOrDefault(x => x.TestResult == "胶路不良")?.qty;
                            var sjjblQty = ResultQty.Where(x => x.hour == hour).FirstOrDefault(x => x.TestResult == "塑胶件不良")?.qty;

                            var sumhourQty = Convert.ToDecimal(sumQty);

                            var jmbulQty = Convert.ToDouble((jmblQty / sumhourQty) * 100).ToString("F2") + "%";
                            var jlbllQty = Convert.ToDouble((jlblQty / sumhourQty) * 100).ToString("F2") + "%";
                            var sjjbllQty = Convert.ToDouble((sjjblQty / sumhourQty) * 100).ToString("F2") + "%";

                            await _databaseService.UpdateQtys(jmbulQty, hour, "键帽不良率%", now.Date);
                            await _databaseService.UpdateQtys(jlbllQty, hour, "胶路不良率%", now.Date);
                            await _databaseService.UpdateQtys(sjjbllQty, hour, "塑胶件不良率%", now.Date);
                        }

                        {
                            var sumQty = ResultQty.Sum(x => x.qty);
                            await _databaseService.UpdateQtys(sumQty.ToString(), "投入数", now.Date);

                            var jmblQty = ResultQty.Where(x => x.TestResult == "键帽不良")?.Sum(x => x.qty);
                            var jlblQty = ResultQty.Where(x => x.TestResult == "胶路不良")?.Sum(x => x.qty);
                            var sjjblQty = ResultQty.Where(x => x.TestResult == "塑胶件不良")?.Sum(x => x.qty);
                            var passQty = ResultQty.Where(x => x.TestResult == "成品产出")?.Sum(x => x.qty);
                            var ccblQty = ResultQty.Where(x => x.TestResult == "CC不良")?.Sum(x => x.qty);

                            await _databaseService.UpdateQtys(jmblQty.ToString(), "键帽不良", now.Date);
                            await _databaseService.UpdateQtys(jlblQty.ToString(), "胶路不良", now.Date);
                            await _databaseService.UpdateQtys(sjjblQty.ToString(), "塑胶件不良", now.Date);
                            await _databaseService.UpdateQtys(passQty.ToString(), "成品产出", now.Date);
                            await _databaseService.UpdateQtys(ccblQty.ToString(), "CC不良", now.Date);


                            var sumhourQty = Convert.ToDecimal(sumQty);

                            var jmbulQty = Convert.ToDouble((jmblQty / sumhourQty) * 100).ToString("F2")+"%";
                            var jlbllQty = Convert.ToDouble((jlblQty / sumhourQty) * 100).ToString("F2") + "%";
                            var sjjbllQty = Convert.ToDouble((sjjblQty / sumhourQty) * 100).ToString("F2") + "%";

                            await _databaseService.UpdateQtys(jmbulQty, "键帽不良率%", now.Date);
                            await _databaseService.UpdateQtys(jlbllQty, "胶路不良率%", now.Date);
                            await _databaseService.UpdateQtys(sjjbllQty, "塑胶件不良率%", now.Date);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"任务异常：{ex.Message}");
            }
        }
    }
}
