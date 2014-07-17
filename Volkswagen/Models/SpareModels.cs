using System;
using System.Web;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Volkswagen.Models
{
    // 用作 EquipmentController 操作的参数的模型。

    [Table("Spares")]
    public class SpareModels
    {

        [Key]
        [Required]
        [Display(Name = "备件物流编号")]
        [StringLength(30)]
        public string SpareID { get; set; }

        [Display(Name = "备件名称")]
        [StringLength(100)]
        public string SpareDes { get; set; }

        [Display(Name = "备件型号")]
        [StringLength(300)]
        public string Type { get; set; }

        [Display(Name = "图片1")]
 //       [StringLength(30)]
        public string Picture1 { get; set; }

        [Display(Name = "图片2")]
        //       [StringLength(30)]
        public string Picture2 { get; set; }

        [Display(Name = "仓位号")]
        [StringLength(50)]
        public string Mark { get; set; }

        [Display(Name = "当前库存")]
 //       [StringLength(50)]
        public int PresentValue { get; set; }

        [Display(Name = "安全库存")]
        //       [StringLength(50)]
        public int SafeValue { get; set; }

        [Display(Name = "东昌最小库存")]
        //       [StringLength(50)]
        public int DCMinValue { get; set; }

        [Display(Name = "东昌最大库存")]
        //       [StringLength(50)]
        public int DCMaxValue { get; set; }

        [Display(Name = "所属设备")]
        [StringLength(50)]
        public string Property { get; set; }

  //      [ForeignKey("Equipments")]
        [Display(Name = "设备编号")]
        [StringLength(15)]
        public string EquipmentID { get; set; }

        [Display(Name = "备件供货商")]
        [StringLength(50)]
        public string Producer { get; set; }

        [Display(Name = "订货号")]
        [StringLength(50)]
        public string OrderNumber { get; set; }

        [Display(Name = "备注")]
        [Column(TypeName = "ntext")]
        public string Remark { get; set; }

        [Display(Name = "设备关键属性")]
        [StringLength(15)]
        public string KeyPart { get; set; }


        [Display(Name = "文件")]
  //      [StringLength(10)]
        public string File { get; set; }

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
        public virtual ICollection<SpareUserModels> SpareUsers { get; set; }
      


    }
}