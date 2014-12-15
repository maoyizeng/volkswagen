using System;
using System.Web;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Volkswagen.Models
{
    // 用作 EquipmentController 操作的参数的模型。

    [Table("Maintains")]
    // 表9 设备维护及状态跟踪
    public class MaintainModels
    {

        
        [Display(Name = "设备编号")]
        [StringLength(15)]
        public string EquipmentID { get; set; }

     
        [Display(Name = "设备名称")]
        [StringLength(50)]
        public string EquipDes { get; set; }

        [Display(Name = "车间生产线")]
        //[StringLength(5)]
        public Nullable<EquipmentModels.WSNames> Line { get; set; }

        [Display(Name = "维护类别")]
        [StringLength(15)]
        public string MType { get; set; }

        [Display(Name = "维护部位")]
        [StringLength(50)]
        public string MPart { get; set; }

        [Display(Name = "维护内容")]
        [Column(TypeName = "ntext")]
        public string Content { get; set; }

        //该字段(保养周期)不在表清单中
        [Display(Name = "保养周期")]
        [StringLength(50)]
        public string Period { get; set; }

        [Display(Name = "维护开始时间")]
        public Nullable<System.DateTime> MStartTime { get; set; }

        [Display(Name = "维护完成时间")]
        public Nullable<System.DateTime> MEndTime { get; set; }

        [Display(Name = "责任班组")]
        //[StringLength(3)]
        public Nullable<RepairModels.ClassType> ResponseClass { get; set; }

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
        public Nullable<int> Mark { get; set; }

        [Display(Name = "等级")]
        [StringLength(10)]
        public string Grade { get; set; }

        [Display(Name = "问题状态")]
        [StringLength(10)]
        public string ProblemStatus { get; set; }

        [Display(Name = "检查次数")]
        public Nullable<int> CheckNum { get; set; }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
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


        //      表连接关系
        public virtual EquipmentModels Equipments { get; set; }

        public void upcast(ArMaintainModels md)
        {
            EquipmentID = md.EquipmentID;
            EquipDes = md.EquipDes;
            Line = md.Line;
            MType = md.MType;
            MPart = md.MPart;
            Content = md.Content;
            Period = md.Period;             //该字段不在数据库清单中
            MStartTime = md.MStartTime;
            MEndTime = md.MEndTime;
            ResponseClass = md.ResponseClass;
            CheckStatus = md.CheckStatus;
            CheckDetail = md.CheckDetail;
            EquipStatus = md.EquipStatus;
            EquipDetail = md.EquipDetail;
            CheckerType = md.CheckerType;
            Checker = md.Checker;
            CheckTime = md.CheckTime;
            Problem = md.Problem;
            Mark = md.Mark;
            Grade = md.Grade;
            ProblemStatus = md.ProblemStatus;
            CheckNum = md.CheckNum;
            MaintainId = md.MaintainId;
            ChangeTime = md.ChangeTime;
            Changer = md.Changer;
            CreateTime = md.CreateTime;
            Creator = md.Creator;

        }
    }
}