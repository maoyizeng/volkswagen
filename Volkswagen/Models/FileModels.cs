﻿using System;
using System.Web;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Volkswagen.Models
{

    [Table("Files")]
    // 表12 文件库
    public class FileModels
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Display(Name = "文件名")]
//        [StringLength(255)]
        public string FileName { get; set; }

        [Display(Name = "类别")]
        [StringLength(30)]
        public string Class { get; set; }

        [Display(Name = "设备编号")]
        [StringLength(15)]
        public string EquipmentID { get; set; }

        [Display(Name = "设备名称")]
        [StringLength(50)]
        public string EquipDes { get; set; }

        [Display(Name = "文件负责人")]
        [StringLength(30)]
        public string Charger { get; set; }

        [Display(Name = "文件")]
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


 //       public virtual EquipmentModels Equipments { get; set; }

        public void upcast(ArFileModels md)
        {
            FileName = md.FileName;
            Class = md.Class;
            EquipmentID = md.EquipmentID;
            EquipDes = md.EquipDes;
            Charger = md.Charger;
            File = md.File;
            ChangeTime = md.ChangeTime;
            Changer = md.Changer;
            CreateTime = md.CreateTime;
            Creator = md.Creator;
        }
    }
}