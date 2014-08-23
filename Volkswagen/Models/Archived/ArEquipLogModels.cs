using System;
using System.Web;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Volkswagen.Models
{
    // 用作 EquipmentController 操作的参数的模型。

    [Table("ArEquipLogs")]
    // 表8 设备台账
    public class ArEquipLogModels
    {
        [Key]
 //       [ForeignKey("ArEquipments")]
        [Required]
        [Display(Name = "设备编号")]
        [StringLength(15, MinimumLength = 7)]
        public string EquipmentID { get; set; }

        [Display(Name = "使用部门")]
        [StringLength(10)]
        public string Department { get; set; }

        [Display(Name = "设备名称")]
        [StringLength(50)]
        public string EquipDes { get; set; }

        [Display(Name = "型号")]
        [StringLength(80)]
        public string Type { get; set; }

        [Display(Name = "规格")]
        [StringLength(80)]
        public string Spec { get; set; }

        [Display(Name = "立卡时间")]
        public Nullable<System.DateTime> DocumentDate { get; set; }

        [Display(Name = "启用时间")]
        public Nullable<System.DateTime> EnableDate { get; set; }

        [Display(Name = "原值")]
        public decimal OriginValue { get; set; }

        [Display(Name = "累计折旧")]
        public decimal Depreciation { get; set; }

        [Display(Name = "一级地点")]
        [StringLength(30)]
        public string Spot1 { get; set; }

        [Display(Name = "二级地点")]
        [StringLength(30)]
        public string Spot2 { get; set; }

        [Display(Name = "三级地点")]
        [StringLength(50)]
        public string Spot3 { get; set; }


        [Display(Name = "备注")]
        [Column(TypeName = "ntext")]
        public string Remark { get; set; }
        
        [Display(Name = "最后修改时间")]
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
        //      表连接关系
        //public virtual ArEquipmentModels ArEquipments { get; set; }

        public ArEquipLogModels(EquipLogModels md)
        {
            EquipmentID = md.EquipmentID;
            Department = md.Department;
            EquipDes = md.EquipDes;
            Type = md.Type;
            Spec = md.Spec;
            DocumentDate = md.DocumentDate;
            EnableDate = md.EnableDate;
            OriginValue = md.OriginValue;
            Depreciation = md.Depreciation;
            Spot1 = md.Spot1;
            Spot2 = md.Spot2;
            Spot3 = md.Spot3;
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