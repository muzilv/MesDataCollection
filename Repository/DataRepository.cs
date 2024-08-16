using Dapper;
using System.Data;
using System.Threading.Tasks;
using System;
using System.Linq;
using MesDataCollection.Model;
using MySql.Data.MySqlClient;
using MesDataCollection.Controllers;
using Microsoft.Extensions.Logging;
using System.Net.NetworkInformation;
using System.Collections.Generic;
using Mysqlx.Crud;

namespace MesDataCollection.Repository
{
    public class DataRepository: BaseRepository
    {
        public DataRepository()
        {
        }

        public async Task SaveUploadModelAsync(UploadModel model,string result)
        {
            using (var connection = GetMySqlConnection())
            {
                await connection.ExecuteAsync(
                    "INSERT INTO Mes_UploadData (LineName, EquipmentId, ProductModel, ProcessName, JigBarCode, TestResult, TestTime) " +
                    "VALUES (@LineName, @EquipmentId, @ProductModel, @ProcessName, @JigBarCode, @TestResult, @TestTime)",
                    new
                    {
                        LineName = model.LineName,
                        EquipmentId = model.EquipmentId,
                        ProductModel = model.ProductModel,
                        ProcessName = model.ProcessName,
                        JigBarCode = model.JigBarCode,
                        TestResult = result,
                        TestTime = model.TestTime
                    });
            }
        }


        public async Task<UploadQty> GetUploadDataQty(DateTime start_time,DateTime end_time)
        {
            using (var connection = GetMySqlConnection())
            {
                var result = await connection.QueryAsync<UploadQty>(
                      "select  count(1) qty from Mes_UploadData where TestTime>=@start_time and TestTime<=@end_time",
                      new
                      {
                          start_time= start_time,
                          end_time = end_time
                      });
                return result.FirstOrDefault();
            }
        }

        public async Task<List<UploadStatus>> GetTestResultQty(DateTime start_time, DateTime end_time)
        {
            using (var connection = GetMySqlConnection())
            {
                var result = await connection.QueryAsync<UploadStatus>(
                      "SELECT TestResult, DATE_FORMAT(TestTime, '%H') AS hour, COUNT(*) AS qty " +
                      "FROM mes_uploaddata where TestTime>@start_time and TestTime<@end_time " +
                      "GROUP BY hour,TestResult " +
                      "ORDER BY hour;",
                      new
                      {
                          start_time = start_time,
                          end_time = end_time
                      });
                return result.ToList();
            }
        }

        public async Task<List<UploadResult>> UploadResult(DateTime start_time, DateTime end_time)
        {
            using (var connection = GetMySqlConnection())
            {
                var result = await connection.QueryAsync<UploadResult>(
                      "SELECT TestResult, COUNT(*) AS qty " +
                      "FROM mes_uploaddata where TestTime>@start_time and TestTime<@end_time " +
                      "GROUP BY TestResult ;",
                      new
                      {
                          start_time = start_time,
                          end_time = end_time
                      });
                return result.ToList();
            }
        }

        public async Task UpdateQty(UploadStatus model, DateTime dt)
        {
            using (var connection = GetMySqlConnection())
            {
                await connection.ExecuteAsync(
                    "update mes_sumdata set `" + model.hour + "`=@qty where projectname =@projectname  and data =@dt",
                    new
                    {
                        projectname = model.TestResult,
                        dt = dt.ToString("yyyy-MM-dd"),
                        qty = model.qty,
                    });
            }
        }


        public async Task UpdateQtys(string  qty,int hour,string projectname, DateTime dt)
        {
            using (var connection = GetMySqlConnection())
            {
                await connection.ExecuteAsync(
                    "update mes_sumdata set `" + hour + "`=@qty where projectname =@projectname  and data =@dt",
                    new
                    {
                        projectname = projectname,
                        dt = dt.ToString("yyyy-MM-dd"),
                        qty = qty,
                    });
            }
        }

        public async Task UpdateQtys(string qty, string projectname, DateTime dt)
        {
            using (var connection = GetMySqlConnection())
            {
                await connection.ExecuteAsync(
                    "update mes_sumdata set `sum_qty`=@qty where projectname =@projectname  and data =@dt",
                    new
                    {
                        projectname = projectname,
                        dt = dt.ToString("yyyy-MM-dd"),
                        qty = qty,
                    });
            }
        }



        public async Task CreateProductionPlan(ProductionPlan model)
        {
            using (var connection = GetMySqlConnection())
            {
                await connection.ExecuteAsync(
                    "INSERT INTO mes_plan ( plan_name, start_time, end_time, plan_quantity, product_model,`desc`) VALUES  " +
                    " (@plan_name, @start_time, @end_time, @plan_quantity, @product_model,@desc)",
                    new
                    {
                        plan_name = model.Plan_Name,
                        start_time = model.Start_Time.AddHours(8),
                        end_time = model.End_Time.AddHours(8),
                        plan_quantity = model.Plan_Quantity,
                        product_model = model.Product_Model,
                        desc=model.desc
                    });
            }
        }

        public async Task<List<ProductionPlan>> GetMesPlan(DateTime dt)
        {
            using (var connection = GetMySqlConnection())
            {
                var result = await connection.QueryAsync<ProductionPlan>(
                      "select* from  mes_plan where is_delete=0 and start_time<@dt and end_time>@dt",
                      new
                      {
                          dt = dt.ToString("yyyy-MM-dd HH:mm:ss")
                      });
                return result.ToList();
            }
        }

        public async Task<List<ProductionPlan>> GetMesPlans(DateTime dt)
        {
            using (var connection = GetMySqlConnection())
            {
                var result = await connection.QueryAsync<ProductionPlan>(
                      "select* from  mes_plan where is_delete=0 order by start_time desc limit 10",
                      new
                      {
                          dt = dt.ToString("yyyy-MM-dd")
                      });
                return result.ToList();
            }
        }

        public async Task<bool> DeleteMesPlans(string  id)
        {
            using (var connection = GetMySqlConnection())
            {
                var result = await connection.ExecuteAsync("delete from mes_plan where  id=@id",
                    new
                    {
                        id = id,
                    });

                 return result>0;
            }
        }


        public async Task UpdateMesPlan(ProductionPlan model)
        {
            using (var connection = GetMySqlConnection())
            {
                await connection.ExecuteAsync(
                    "update mes_plan set ActualQty=@ActualQty where id=@Id",
                    new
                    {
                        ActualQty = model.ActualQty,
                        id = model.Id
                    });
            }
        }



        public async Task<bool> TableExistsAsync()
        {
            using (var connection = GetMySqlConnection())
            {
                var result = await connection.QueryAsync<int>(
                    "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Mes_UploadData'");
                return result.FirstOrDefault() > 0;
            }
        }

        public async Task CreateTableAsync()
        {
            using (var connection = GetMySqlConnection())
            {
                await connection.ExecuteAsync(
                    "CREATE TABLE Mes_UploadData (" +
                    "Id INT AUTO_INCREMENT PRIMARY KEY," +
                    "LineName VARCHAR(255) NOT NULL," +
                    "EquipmentId VARCHAR(255) NOT NULL," +
                    "ProductModel VARCHAR(255) NOT NULL," +
                    "ProcessName VARCHAR(255) NOT NULL," +
                    "JigBarCode VARCHAR(255) NOT NULL," +
                    "TestResult TEXT NOT NULL," +
                    "TestTime DATETIME NOT NULL" +
                    ")");
            }
        }

        public async Task<bool> GetSumData(DateTime date)
        {
            using (var connection = GetMySqlConnection())
            {
                var result = await connection.QueryAsync<int>( "SELECT COUNT(*) FROM mes_sumdata WHERE data =@date", new { date = date.ToString("yyyy-MM-dd") });
                return result.FirstOrDefault() > 0;
            }
        }

        public async Task CreateSumData(DateTime date)
        {
            using (var connection = GetMySqlConnection())
            {
                await connection.ExecuteAsync(@"insert into mes_sumdata ( projectname, `data`, `0`, `1`, `2`, `3`, `4`, `5`, `6`, `7`, `8`, `9`, `10`, `11`,`12`, `13`, `14`, `15`, `16`, `17`, `18`, `19`, `20`, `21`, `22`, `23`, sum_qty, sort) values
('成品产出', @data, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 9),
('CC不良', @data, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 8),
('塑胶不良率%', @data, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 7),
('塑胶不良', @data, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 6),
('胶路不良率%', @data, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 5),
('胶路不良', @data, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 4),
('键帽不良率%', @data, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3),
('键帽不良', @data, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2),
('投入数', @data, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1);",
                    new
                    {
                        data = date.ToString("yyyy-MM-dd")
                    });
            }
        }


        public async Task<List<TimeData>> GetMesSumData()
        {
            using (var connection = GetMySqlConnection())
            {
                var result = await connection.QueryAsync<TimeData>(
                      "select  projectname 'Project',`8` as 'Time8',`9` 'Time9',`10` 'Time10',`11` 'Time11',`12` 'Time12',`13` 'Time13',`14` 'Time14',`15` 'Time15',`16` 'Time16',`17` 'Time17',`18` 'Time18',`19` 'Time19',`20` 'Time20',`21` 'Time21',`22` 'Time22',`23` 'Time23',`0` 'Time0',`1` 'Time1',`2` 'Time2',`3` 'Time3',`4` 'Time4',`5` 'Time5',`6` 'Time6',`7` 'Time7' from mes_sumdata where data=@dt order by sort asc",
                      new
                      {
                          dt = DateTime.Now.ToString("yyyy-MM-dd")
                      });
                return result.ToList();
            }
        }

        public async Task<List<PlanQty>> GetMesPlan(DateTime start_time, DateTime end_time)
        {
            using (var connection = GetMySqlConnection())
            {
                var result = await connection.QueryAsync<PlanQty>(
                      "select  start_time,plan_name,plan_quantity,ActualQty from mes_plan where  start_time>=@start_time and start_time<=@end_time",
                      new
                      {
                          start_time = start_time,
                          end_time = end_time
                      });
                return result.ToList();
            }
        }


        public async Task<DayQty> GetDayQty()
        {
            using (var connection = GetMySqlConnection())
            {
                var result = await connection.QueryAsync<DayQty>(
                      "select  `8` as 'Time8',`9` 'Time9',`10` 'Time10',`11` 'Time11',`12` 'Time12',`13` 'Time13',`14` 'Time14',`15` 'Time15',`16` 'Time16',`17` 'Time17',`18` 'Time18',`19` 'Time19',`20` 'Time20',`21` 'Time21',`22` 'Time22',`23` 'Time23',`0` 'Time0',`1` 'Time1',`2` 'Time2',`3` 'Time3',`4` 'Time4',`5` 'Time5',`6` 'Time6',`7` 'Time7' from mes_sumdata where projectname='成品产出' and data=@dt limit 1",
                      new
                      {
                          dt = DateTime.Now.ToString("yyyy-MM-dd")
                      });
                return result.FirstOrDefault();
            }
        }

        public async Task<OutputStatistics> GetOutputStatistics(DateTime start_time, DateTime end_time)
        {
            using (var connection = GetMySqlConnection())
            {
                var result = await connection.QueryAsync<OutputStatistics>(
                      "select sum(1) TotalQty,sum(CASE TestResult WHEN '成品产出' THEN 1 ELSE 0 END) PassQty from  mes_uploaddata where  TestTime>=@start_time and TestTime<=@end_time",
                      new
                      {
                          start_time = start_time,
                          end_time = end_time
                      });
                return result.FirstOrDefault();
            }
        }

        public async Task<ProductionPlan> GetMesPlanDefault(DateTime start, DateTime end)
        {
            using (var connection = GetMySqlConnection())
            {
                var result = await connection.QueryAsync<ProductionPlan>(
                      "select* from  mes_plan where is_delete=0 and start_time>=@start_time and end_time<=@end_time order by id desc limit 1",
                      new
                      {
                          start_time = start,
                          end_time= end
                      });
                return result.FirstOrDefault();
            }
        }



        public async Task<bool> CheckConnectionAsync()
        {
            try
            {
                using (var connection = GetMySqlConnection())
                {

                    await connection.OpenAsync();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

            }
            return false;
        }

        public async Task<bool> CheckIPAsync(string ipAddress)
        {
            var ping = new Ping();
            var reply = await ping.SendPingAsync(ipAddress);
            return reply.Status == IPStatus.Success;
        }
    }
}
