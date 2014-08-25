using System;
using System.Web;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Volkswagen.Models
{

    [Table("Repairs")]
    // 表6 设备报修单
    public class RepairModels
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


        public virtual EquipmentModels Equipments { get; set; }

        public void upcast(ArRepairModels md)
        {
            SheetID = md.SheetID;
            RepairID = md.RepairID;
            EquipDes = md.EquipDes;
            EquipmentID = md.EquipmentID;
            StartTime = md.StartTime;
            FinishTime = md.FinishTime;
            RepairTime = md.RepairTime;
            Class = md.Class;
            Line = md.Line;
            Section = md.Section;
            FaultView = md.FaultView;
            Repairman = md.Repairman;
            Description = md.Description;
            FaultType = md.FaultType;
            Result = md.Result;
            Problem = md.Problem;
            Checker = md.Checker;
            Remark = md.Remark;
            StopTime = md.StopTime;
            RepairNum = md.RepairNum;
            ChangeTime = md.ChangeTime;
            Changer = md.Changer;
            CreateTime = md.CreateTime;
            Creator = md.Creator;


        }
    }
}