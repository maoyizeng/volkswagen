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
        [Display(Name = "报修单号")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [StringLength(15)]
        public string SheetID { get; set; }

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

        public enum ClassType
        {
            A,
            B,
            C
        }

        [Display(Name = "班次")]
        //[StringLength(5)]
        public Nullable<ClassType> Class { get; set; }

        [Display(Name = "车间生产线")]
        //[StringLength(5)]
        public Nullable<EquipmentModels.WSNames> Line { get; set; }

        public enum SectionNames
        {
            
            一工段,
            二工段,
            三工段,
            四工段,
            返修
        }

        [Display(Name = "工段")]
        //[StringLength(10)]
        public Nullable<SectionNames> Section { get; set; }

        [Display(Name = "故障现象")]
        [StringLength(200)]
        public string FaultView { get; set; }

        [Display(Name = "维修人")]
        [StringLength(30)]
        public string Repairman { get; set; }

        [Display(Name = "故障原因和维修内容")]
        [StringLength(600)]
        public string Description { get; set; }

        public enum FaultTypeEnum
        {
            传动系统,
            润滑系统,
            紧固部件,
            液压部件,
            气压部件,
            水压部件,
            主电路,
            控制电路,
            程控器,
            输送系统,
            计量系统,
            制动系统,
            滑块系统,
            过滤器,
            安全系统,
            其它
        }

        [Display(Name = "故障类别")]
        //[StringLength(10)]
        public Nullable<FaultTypeEnum> FaultType { get; set; }

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


        public virtual EquipmentModels Equipments { get; set; }

        public void upcast(ArRepairModels md)
        {
            SheetID = md.SheetID;
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