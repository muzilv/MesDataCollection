using System;

namespace MesDataCollection
{
    public class ProductionPlan
    {
        public int Id { get; set; } // 主键

        public string Plan_Name { get; set; } // 计划名称

        public DateTime Start_Time { get; set; } // 开始时间

        public DateTime End_Time { get; set; } // 结束时间

        public string Plan_Quantity { get; set; } // 计划数量

        public string Product_Model { get; set; } // 生产型号

        public DateTime CreatedAt { get; set; } = DateTime.Now; // 创建时间

        public long ActualQty { get; set; }

        public int is_delete { get; set; }

        public string desc { get; set; }
    }
}
