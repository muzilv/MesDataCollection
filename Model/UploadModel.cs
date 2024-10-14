using System.Collections.Generic;
using System;
using Newtonsoft.Json;

namespace MesDataCollection.Model
{
    public class UploadModel
    {
        /// <summary>
        /// 线体名称
        /// </summary>
        public string LineName { get; set; }
        /// <summary>
        /// 设备ID    
        /// </summary>
        public string EquipmentId { get; set; }
        /// <summary>
        /// 产品型号    
        /// </summary>
        public string ProductModel { get; set; }
        /// <summary>
        /// 工序名称
        /// </summary>
        public string ProcessName { get; set; }
        /// <summary>
        /// 条码
        /// </summary>
        public string JigBarCode { get; set; }
        /// <summary>
        /// 测试结果
        /// </summary>
        public List<string> TestResult { get; set; }
        /// <summary>
        /// 测试时间
        /// </summary>
        public DateTime TestTime { get; set; }
    }


    public class UploadQty
    {
        /// <summary>
        /// 线体名称
        /// </summary>
        public long qty { get; set; }

    }

    public class UploadStatus
    {
        public string TestResult { get; set; }

        public long qty { get; set; }

        public int hour { get; set; }

    }

    public class ProcessQty
    {
        public string ProcessName { get; set; }

        public string TestResult { get; set; }

        public long qty { get; set; }

        public int hour { get; set; }

    }

    public class UploadResult
    {
        public string TestResult { get; set; }

        public long qty { get; set; }


    }


    public class TimeData
    {
        [JsonProperty("项目")]
        public string Project { get; set; }

        // 为每个时间点创建属性，并使用JsonProperty来映射JSON键
        [JsonProperty("8:00")]
        public string Time8 { get; set; }

        [JsonProperty("9:00")]
        public string Time9 { get; set; }

        [JsonProperty("10:00")]
        public string Time10 { get; set; }
        [JsonProperty("11:00")]
        public string Time11 { get; set; }
        [JsonProperty("12:00")]
        public string Time12 { get; set; }
        [JsonProperty("13:00")]
        public string Time13 { get; set; }
        [JsonProperty("14:00")]
        public string Time14 { get; set; }
        [JsonProperty("15:00")]
        public string Time15 { get; set; }
        [JsonProperty("16:00")]
        public string Time16 { get; set; }
        [JsonProperty("17:00")]
        public string Time17 { get; set; }
        [JsonProperty("18:00")]
        public string Time18 { get; set; }
        [JsonProperty("19:00")]
        public string Time19 { get; set; }
        [JsonProperty("20:00")]
        public string Time20 { get; set; }
        [JsonProperty("21:00")]
        public string Time21 { get; set; }
        [JsonProperty("22:00")]
        public string Time22 { get; set; }

        [JsonProperty("23:00")]
        public string Time23 { get; set; }

        [JsonProperty("0:00")]
        public string Time0 { get; set; }

        [JsonProperty("1:00")]
        public string Time1 { get; set; }
        [JsonProperty("2:00")]
        public string Time2 { get; set; }
        [JsonProperty("3:00")]
        public string Time3 { get; set; }
        [JsonProperty("4:00")]
        public string Time4 { get; set; }
        [JsonProperty("5:00")]
        public string Time5 { get; set; }

        [JsonProperty("6:00")]
        public string Time6 { get; set; }

        [JsonProperty("7:00")]
        public string Time7 { get; set; }


    }

    public class DayQty
    {
        // 为每个时间点创建属性，并使用JsonProperty来映射JSON键
        [JsonProperty("8:00")]
        public string Time8 { get; set; }

        [JsonProperty("9:00")]
        public string Time9 { get; set; }

        [JsonProperty("10:00")]
        public string Time10 { get; set; }
        [JsonProperty("11:00")]
        public string Time11 { get; set; }
        [JsonProperty("12:00")]
        public string Time12 { get; set; }
        [JsonProperty("13:00")]
        public string Time13 { get; set; }
        [JsonProperty("14:00")]
        public string Time14 { get; set; }
        [JsonProperty("15:00")]
        public string Time15 { get; set; }
        [JsonProperty("16:00")]
        public string Time16 { get; set; }
        [JsonProperty("17:00")]
        public string Time17 { get; set; }
        [JsonProperty("18:00")]
        public string Time18 { get; set; }
        [JsonProperty("19:00")]
        public string Time19 { get; set; }
        [JsonProperty("20:00")]
        public string Time20 { get; set; }
        [JsonProperty("21:00")]
        public string Time21 { get; set; }
        [JsonProperty("22:00")]
        public string Time22 { get; set; }

        [JsonProperty("23:00")]
        public string Time23 { get; set; }

        [JsonProperty("0:00")]
        public string Time0 { get; set; }

        [JsonProperty("1:00")]
        public string Time1 { get; set; }
        [JsonProperty("2:00")]
        public string Time2 { get; set; }
        [JsonProperty("3:00")]
        public string Time3 { get; set; }
        [JsonProperty("4:00")]
        public string Time4 { get; set; }
        [JsonProperty("5:00")]
        public string Time5 { get; set; }

        [JsonProperty("6:00")]
        public string Time6 { get; set; }

        [JsonProperty("7:00")]
        public string Time7 { get; set; }


    }

    public class ProjectnameQty
    {
        public string projectname { get; set; }

        public DateTime data { get; set; }

        public string  sum_qty { get; set; }

    }


    public class CapsuleChart
    {
        public string name { get; set; }

        public long value { get; set; }

    }


    public class OutputStatistics
    {
        /// <summary>
        /// 计划数量
        /// </summary>
        public long PlannedQty { get; set; } = 0;
        /// <summary>
        /// 预计数量
        /// </summary>
        public long ForecastQty { get; set; }
        /// <summary>
        /// 当前投入
        /// </summary>
        public long TotalQty { get; set; }
        /// <summary>
        /// 成品数量
        /// </summary>
        public long PassQty { get; set; }
        /// <summary>
        /// 不良数量
        /// </summary>
        public long FialQty { get; set; }
       
    }


    public class PlanQty
    {
        public DateTime start_time { get; set; }
        public string plan_name { get; set; }
        public long plan_quantity { get; set; }
        public long ActualQty { get; set; }
    }


    public class LineModel
    {
        public string LineName { get; set; } 
    }

    public class boxmodel
    {
        public string value { get; set; }
        public string label { get; set; }
        
    }
}
