using System;
using System.Web;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Volkswagen.Models
{
    // 用作 EquipmentController 操作的参数的模型。

    [Table("ArMaintains")]
    // 表9 设备维护及状态跟踪
    public class ArMaintainModels
    {

        
        [Display(Name = "设备编号")]
        [StringLength(15)]
        public string EquipmentID { get; set; }

     
        [Display(Name = "设备名称")]
        [StringLength(50)]
        public string EquipDes { get; set; }

        [Display(Name = "车间生产线")]
        [StringLength(5)]
        public string Line { get; set; }

        [Display(Name = "维护类别")]
        [StringLength(15)]
        public string MType { get; set; }

        [Display(Name = "维护部位")]
        [StringLength(50)]
        public string MPart { get; set; }

        [Display(Name = "维护内容")]
        [Column(TypeName = "ntext")]
        public string Content { get; set; }

        [Display(Name = "保养周期")]
        [StringLength(50)]
        public string Period { get; set; }

        [Display(Name = "维护开始时间")]
        public Nullable<System.DateTime> MStartTime { get; set; }

        [Display(Name = "维护完成时间")]
        public Nullable<System.DateTime> MEndTime { get; set; }

        [Display(Name = "责任班组")]
        [StringLength(3)]
        public string ResponseClass { get; set; }

        [Display(Name = "检具状态")]
        [StringLength(20)]
        public string CheckStatus { get; set; }

        [Display(Name = "检具详细状况")]
        [Column(TypeName = "ntext")]
        public string CheckDetail { get; set; }

        [Display(Name = "设备状态")]
        [StringLength(20)]
        public string EquipStatus { get; set; }

        [Display(Name = "设备详细状况")]
        [Column(TypeName = "ntext")]
        public string EquipDetail { get; set; }

        [Display(Name = "检查人类别")]
        [StringLength(10)]
        public string CheckerType { get; set; }

        [Display(Name = "检查人")]
        [StringLength(20)]
        public string Checker { get; set; }

        [Display(Name = "检查时间")]
        public Nullable<System.DateTime> CheckTime { get; set; }

        [Display(Name = "问题")]
        [Column(TypeName = "ntext")]
        public string Problem { get; set; }

        [Display(Name = "分数")]
        public int Mark { get; set; }

        [Display(Name = "等级")]
        [StringLength(10)]
        public string Grade { get; set; }

        [Display(Name = "问题状态")]
        [StringLength(10)]
        public string ProblemStatus { get; set; }

        [Display(Name = "检查次数")]
        public int CheckNum{ get; set; }

        [Key]
        [Display(Name = "编号")]
 //       [StringLength(15)]
        public int MaintainId { get; set; }

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
        [Key]
        [Display(Name = "操作类型")]
        [StringLength(10)]
        public string Operator { get; set; }

        //      表连接关系
   //     public virtual ArEquipmentModels ArEquipments { get; set; }


    }
}