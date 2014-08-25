﻿using System;
using System.Web;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Volkswagen.Models
{

    [Table("ArShifts")]
    // 表7 交班记录
    public class ArShiftModels
    {
        [Key]
        [Required]
        [Display(Name = "记录编号")]
        [StringLength(15)]
        public string ShiftID { get; set; }

        [Display(Name = "交班日期")]
  //      [Column(TypeName = "date")]
        public Nullable<System.DateTime> ShiftDate { get; set; }

        [Display(Name = "交班时间")]
  //      [Column(TypeName = "time")]
        public Nullable<System.DateTime> ShiftTime { get; set; }

        [Display(Name = "班次")]
        [StringLength(5)]
        public string Class { get; set; }

        [Display(Name = "车间生产线")]
        [StringLength(5)]
        public string Line { get; set; }

        [Display(Name = "负责人")]
        [StringLength(10)]
        public string Charger { get; set; }

        [Display(Name = "情况记录")]
        [Column(TypeName = "ntext")]
        public string Record { get; set; }

        [Display(Name = "紧急事件")]
        [Column(TypeName = "ntext")]
        public string Urgency { get; set; }

        [Display(Name = "备注")]
        [Column(TypeName = "ntext")]
        public string Remark { get; set; }

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
 //       public virtual EquipmentModels Equipments { get; set; }

        public ArShiftModels() { }

        public ArShiftModels (ShiftModels md)
        {
            ShiftID = md.ShiftID;
            ShiftDate = md.ShiftDate;
            ShiftTime = md.ShiftTime;
            Class = md.Class;
            Line = md.Line;
            Charger = md.Charger;
            Record = md.Record;
            Urgency = md.Urgency;
            Remark = md.Remark;
            ChangeTime = md.ChangeTime;
            Changer = md.Changer;
            CreateTime = md.CreateTime;
            Creator = md.Creator;

            Operator = "Default";
            OperateTime = DateTime.Now;
        }
    }
}