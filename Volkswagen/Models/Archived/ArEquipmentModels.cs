using System;
using System.Web;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Volkswagen.Models
{
    // 用作 EquipmentController 操作的参数的模型。

    [Table("ArEquipments")]
    // 表1 设备履历
    public class ArEquipmentModels
    {
        [Key]
        [Required]
        [Display(Name = "设备编号")]
        [StringLength(15)]
        public string EquipmentID { get; set; }

        [Required]
        [Display(Name = "设备名称")]
        [StringLength(50)]
        public string EquipDes { get; set; }

        [Display(Name = "负责人")]
        [StringLength(10)]
        public string Person { get; set; }

        [Display(Name = "所在工段")]
        [StringLength(30)]
        public string Section { get; set; }

        [Display(Name = "车间生产线")]
        [StringLength(15)]
        public string WSArea { get; set; }

        [Display(Name = "设备照片")]
 //       [StringLength(10)]
        public string Photo { get; set; }

        [Display(Name = "点检")]
        [StringLength(15)]
        public string ItemInspect { get; set; }

        [Display(Name = "日常保养")]
        [StringLength(15)]
        public string RegularCare { get; set; }

        [Display(Name = "巡检")]
        [StringLength(15)]
        public string Check { get; set; }

        [Display(Name = "需更新否")]
        [StringLength(15)]
        public string RoutingInspect { get; set; }

  //      [Display(Name = "TPM文件")]
  //      [StringLength(10)]
  //      public string TPMFile { get; set; }

  //      [Display(Name = "操作规程")]
  //      [StringLength(10)]
  //      public string Rules { get; set; }

  //      [Display(Name = "技术文件")]
  //      [StringLength(10)]
  //      public string TechnicFile { get; set; }

  //      [Display(Name = "培训资料")]
  //      [StringLength(10)]
  //      public string TrainingFile { get; set; }

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

        [Display(Name = "备注")]
  //      [StringLength(10)]
        [Column(TypeName = "ntext")]
        public string Remark { get; set; }


        public virtual ICollection<ArSpareModels> ArSpares { get; set; }
        public virtual ICollection<ArInspectionModels> ArInspections { get; set; }
        public virtual ICollection<ArSpareOrderModels> ArSpareOrders { get; set; }
        public virtual ICollection<ArRepairModels> ArRepairs { get; set; }
        public virtual ArEquipLogModels ArEquipLogs { get; set; }
        public virtual ICollection<ArMaintainModels> ArMaintains { get; set; }
    }
}