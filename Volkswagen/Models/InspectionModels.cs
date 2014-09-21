using System;
using System.Web;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Volkswagen.Models
{
    // 用作 EquipmentController 操作的参数的模型。

    [Table("Inspections")]
    // 表2 设备保养计划
    public class InspectionModels
    {

        [Required]
        [Display(Name = "设备编号")]
        [StringLength(15)]
        public string EquipmentID { get; set; }

        [Required]
        [Display(Name = "设备名称")]
        [StringLength(50)]
        public string EquipDes { get; set; }

        public enum InspectionClass
        {
            保养,
            外保养
        }

        [Display(Name = "维护类别")]
        //[StringLength(30)]
        public Nullable<InspectionClass> Class { get; set; }

        [Display(Name = "保养部件")]
        [StringLength(30)]
        public string Part { get; set; }

        [Display(Name = "保养位置")]
        [StringLength(30)]
        public string Position { get; set; }

        [Display(Name = "保养内容")]
        [StringLength(200)]
        public string Content { get; set; }

        [Display(Name = "保养周期")]
        [StringLength(50)]
        public string Period { get; set; }

        [Display(Name = "注意事项")]
        [Column(TypeName = "ntext")]
        public string Caution { get; set; }

        [Display(Name = "备注")]
        [Column(TypeName = "ntext")]
        public string Remark { get; set; }

        [Key]
        [Display(Name = "编号")]
 //       [StringLength(15)]
        public int InspectionId { get; set; }

  //      [Display(Name = "年度保养文件")]
  //      [StringLength(15)]
  //      public string YInspectionFile { get; set; }

  //      [Display(Name = "月度保养文件")]
  //      [StringLength(15)]
  //      public string MInspectionFile { get; set; }

  //      [Display(Name = "委外保养文件")]
  //      [StringLength(10)]
  //      public string OInspectionFile { get; set; }

  //      [Display(Name = "委外保养文件")]
  //      [StringLength(10)]
  //      public string OtherFile { get; set; }

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


        //      表连接关系
        public virtual EquipmentModels Equipments { get; set; }

        public void upcast(ArInspectionModels md)
        {
            EquipmentID = md.EquipmentID;
            EquipDes = md.EquipDes;
            Class = md.Class;
            Part = md.Part;
            Position = md.Position;
            Content = md.Content;
            Period = md.Period;
            Caution = md.Caution;
            Remark = md.Remark;
            InspectionId = md.InspectionId;
            ChangeTime = md.ChangeTime;
            Changer = md.Changer;
            CreateTime = md.CreateTime;
            Creator = md.Creator;
        }
    }
}