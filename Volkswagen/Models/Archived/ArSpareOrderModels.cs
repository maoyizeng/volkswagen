using System;
using System.Web;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Volkswagen.Models
{
    // 用作 EquipmentController 操作的参数的模型。

    [Table("ArSpareOrders")]
    // 表5 备件订购信息
    public class ArSpareOrderModels
    {
        
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

        [Display(Name = "订购数量")]
        public int OrderValue { get; set; }

        [Display(Name = "备件制造商")]
        [StringLength(50)]
        public string Producer { get; set; }

        [Display(Name = "订货号")]
        [StringLength(50)]
        public string OrderNum { get; set; }

        [Display(Name = "所属设备")]
        [StringLength(50)]
        public string Property { get; set; }

        [Display(Name = "设备编号")]
        [StringLength(15)]
        public string EquipmentID { get; set; }

        [Display(Name = "设备商")]
        [StringLength(40)]
        public string Maker { get; set; }

        [Display(Name = "产品编号")]
        [StringLength(40)]
        public string MakerNum { get; set; }

        [Display(Name = "订购人")]
        [StringLength(10)]
        public string Orderman { get; set; }

        [Display(Name = "订购日期")]
 //       [StringLength(30)]
        public Nullable<System.DateTime> UseTime { get; set; }

        [Display(Name = "单价金额")]
        [Column(TypeName = "bigint")]
        public int UnitPrice { get; set; }

        [Display(Name = "总金额")]
        [Column(TypeName = "bigint")]
        public int TotalPrice { get; set; }

        [Display(Name = "状态")]
        [StringLength(30)]
        public string Status { get; set; }


        [Display(Name = "采购形式")]
        [StringLength(15)]
        public string Mode { get; set; }

    //    [Display(Name = "订购文件")]
    //    public string OrderFile { get; set; }

        [Key]
        [Display(Name = "编号")]
        [Column(TypeName = "bigint")]
        public int OrderID { get; set; }

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
 //       public virtual ArSpareModels ArSpares { get; set; }
 //       public virtual ArEquipmentModels ArEquipments { get; set; }

        public ArSpareOrderModels(SpareOrderModels md)
        {
            SpareID = md.SpareID;
            SpareDes = md.SpareDes;
            Type = md.Type;
            OrderValue = md.OrderValue;
            Producer = md.Producer;
            OrderNum = md.OrderNum;
            Property = md.Property;
            EquipmentID = md.EquipmentID;
            Maker = md.Maker;
            MakerNum = md.MakerNum;
            Orderman = md.Orderman;
            UseTime = md.UseTime;
            UnitPrice = md.UnitPrice;
            TotalPrice = md.TotalPrice;
            Status = md.Status;
            Mode = md.Mode;
            OrderID = md.OrderID;
            ChangeTime = md.ChangeTime;
            Changer = md.Changer;
            CreateTime = md.CreateTime;
            Creator = md.Creator;

            Operator = "Default";
            OperateTime = DateTime.Now;
        }
    }
}