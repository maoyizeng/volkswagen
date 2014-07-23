﻿using System;
using System.Web;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Volkswagen.Models
{
    // 用作 EquipmentController 操作的参数的模型。

    [Table("SpareUsers")]
    public class SpareUserModels
    {
        
        [Required]
        [Display(Name = "备件物流编号")]
        [StringLength(30)]
        public string SpareID { get; set; }

        [Display(Name = "备件名称")]
        [StringLength(100)]
        public string SpareDes { get; set; }

        [Display(Name = "备件型号")]
        [StringLength(300)]
        public string Type { get; set; }

        [Display(Name = "入库数量")]
        //       [StringLength(50)]
        public int InValue { get; set; }

        [Display(Name = "出库数量")]
        //       [StringLength(50)]
        public int OutValue { get; set; }

        [Display(Name = "领用人员")]
        [StringLength(10)]
        public string User { get; set; }

        [Display(Name = "领用时间")]
 //       [StringLength(30)]
        public Nullable<System.DateTime> UseTime { get; set; }

        [Display(Name = "实际使用设备")]
        [StringLength(50)]
        public string ActualUse { get; set; }

        [Key]
        [Display(Name = "编号")]
        [Column(TypeName = "bigint")]
        public int UserID { get; set; }

        [Display(Name = "最后修改时间")]
  //      [StringLength(10)]
        public Nullable<System.DateTime> ChangeTime { get; set; }

        [Display(Name = "修改人")]
        [StringLength(10)]
        public string Changer { get; set; }

        [Display(Name = "创建时间")]
 //       [StringLength(10)]
        public Nullable<System.DateTime> CreateTime { get; set; }

        [Display(Name = "创建人")]
        [StringLength(10)]
        public string Creator { get; set; }


        public virtual SpareModels Spares { get; set; }

    }
}