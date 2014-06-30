using System;
using System.Web;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Volkswagen.Models
{
    // 用作 EquipmentController 操作的参数的模型。

    public class EquipmentModelsDefault1
    {
        [Display(Name = "设备编号")]
        public string EquipmentNumber { get; set; }
        
        //TODO more
    }
}