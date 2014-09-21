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
        public Nullable<int> RepairTime { get; set; }

        [Display(Name = "班次")]
        //[StringLength(5)]
        public Nullable<RepairModels.ClassType> Class { get; set; }

        [Display(Name = "车间生产线")]
        //[StringLength(5)]
        public Nullable<EquipmentModels.WSNames> Line { get; set; }

        [Display(Name = "工段")]
        //[StringLength(10)]
        public Nullable<RepairModels.SectionNames> Section { get; set; }

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
        //[StringLength(10)]
        public Nullable<RepairModels.FaultTypeEnum> FaultType { get; set; }

        [Display(Name = "已修复否")]
        //[StringLength(3)]
        public Nullable<EquipmentModels.YesNo> Result { get; set; }

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
        public Nullable<int> StopTime { get; set; }

  //      [Display(Name = "文件")]
  //      public string File { get; set; }

        [Display(Name = "维修次数")]
        public Nullable<int> RepairNum { get; set; }

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

        [Display(Name = "操作时间")]
        public DateTime OperateTime { get; set; }
  //      public virtual ArEquipmentModels ArEquipments { get; set; }

        public ArRepairModels() { }

        public ArRepairModels(RepairModels md)
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

            Operator = "Default";
            OperateTime = DateTime.Now;


        }

    }
}