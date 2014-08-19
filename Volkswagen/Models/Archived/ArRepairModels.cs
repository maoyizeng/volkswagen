using System;
using System.Web;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Volkswagen.Models
{

    [Table("ArRepairs")]
    // 表6 设备报修单
    public class ArRepairModels
    {
        [Key]
        [Required]
        [Display(Name = "原单据编号")]
        [StringLength(15)]
        public string SheetID { get; set; }

        [Display(Name = "报修单号编号")]
        [StringLength(15)]
        public string RepairID { get; set; }

        [Display(Name = "设备编号")]
        [StringLength(15)]
        public string EquipmentID { get; set; }

        [Required]
        [Display(Name = "设备名称")]
        [StringLength(50)]
        public string EquipDes { get; set; }

        [Display(Name = "报修时间")]
        //       [StringLength(10)]
        public Nullable<System.DateTime> StartTime { get; set; }

        [Display(Name = "修复时间")]
        //       [StringLength(10)]
        public Nullable<System.DateTime> FinishTime { get; set; }

        [Display(Name = "维修耗时")]
        [Column(TypeName = "bigint")]
        public int RepairTime { get; set; }

        [Display(Name = "班次")]
        [StringLength(5)]
        public string Class { get; set; }

        [Display(Name = "车间生产线")]
        [StringLength(5)]
        public string Line { get; set; }

        [Display(Name = "工段")]
        [StringLength(10)]
        public string Section { get; set; }

        [Display(Name = "故障现象")]
        [StringLength(200)]
        public string FaultView { get; set; }

        [Display(Name = "维修人")]
        [StringLength(30)]
        public string Repairman { get; set; }

        [Display(Name = "故障原因和维修内容")]
        [StringLength(600)]
        public string Description { get; set; }

        [Display(Name = "故障类别")]
        [StringLength(10)]
        public string FaultType { get; set; }

        [Display(Name = "已修复否")]
        [StringLength(3)]
        public string Result { get; set; }

        [Display(Name = "遗留问题")]
        [StringLength(200)]
        public string Problem { get; set; }

        [Display(Name = "验收人")]
        [StringLength(10)]
        public string Checker { get; set; }

        [Display(Name = "备注")]
        [StringLength(200)]
        public string Remark { get; set; }

        [Display(Name = "停机时间")]
        [Column(TypeName = "bigint")]
        public int StopTime { get; set; }

  //      [Display(Name = "文件")]
  //      public string File { get; set; }

        [Display(Name = "维修次数")]
        public int RepairNum { get; set; }

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

        //操作类型记录对原表的修改类型: Insert / Delete / Update
        [Display(Name = "操作类型")]
        [StringLength(10)]
        public string Operator { get; set; }

  //      public virtual ArEquipmentModels ArEquipments { get; set; }
    }
}