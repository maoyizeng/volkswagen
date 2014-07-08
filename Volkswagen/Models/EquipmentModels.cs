using System;
using System.Web;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Volkswagen.Models
{
    // 用作 EquipmentController 操作的参数的模型。

    public class EquipmentModels
    {
       // [Display(Name = "设备编号")]
        [KeyAttribute]
        public string EquipmentNumber { get; set; }
        public string EquipId { get; set; }
        public string EquipDes { get; set; }
        public string Person { get; set; }
        public string Section { get; set; }
        public string WSArea { get; set; }
        public string Photo { get; set; }
        public string ItemInspect { get; set; }
        public string RegularCare { get; set; }
        public string Check { get; set; }
        public string RoutingInspect { get; set; }
        public string TPMFile { get; set; }
        public string Rules { get; set; }
        public string TechnicFile { get; set; }
        public string TrainingFile { get; set; }
        public Nullable<System.DateTime> ChangeTime { get; set; }
        public string Changer { get; set; }
        public Nullable<System.DateTime> CreateTime { get; set; }
        public string Creator { get; set; }
        public string Remark { get; set; }

    }
}