using System;
using System.Web;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Volkswagen.Models
{
    // 用作 EquipmentController 操作的参数的模型。

    [Table("Users")]
    // 表10 用户记录

    public class UserModels
    {
        
        
        [Display(Name = "简称")]
        [StringLength(10)]
        public string Breviary { get; set; }

        [Required]
        [Display(Name = "姓名")]
        [StringLength(10)]
        public string Name { get; set; }

        [Display(Name = "工号")]
        [StringLength(10)]
        public string Number { get; set; }

        [Display(Name = "电话")]
        [StringLength(30)]
        public string Telephone { get; set; }

        [Display(Name = "手机")]
        [StringLength(50)]
        public string Mobile { get; set; }

        [Display(Name = "生日")]
        public Nullable<System.DateTime> Birthday { get; set; }

        [Display(Name = "进入公司时间")]
        public Nullable<System.DateTime> EntryDate { get; set; }

        [Display(Name = "职务")]
        [StringLength(20)]
        public string Position { get; set; }

        [Display(Name = "政治面貌")]
        [StringLength(10)]
        public string PoliticalStatus { get; set; }

        [Display(Name = "住址")]
        [StringLength(100)]
        public string Address { get; set; }

        [Display(Name = "技能特长")]
        [StringLength(500)]
        public string Skill { get; set; }

        [Display(Name = "工作经验")]
        [StringLength(1000)]
        public string Experience { get; set; }

        [Display(Name = "备注")]
        [Column(TypeName = "ntext")]
        public string Remark { get; set; }

        [Display(Name = "图片")]
        public string Image { get; set; }

        [Key]
        [Display(Name = "编号")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column(TypeName = "bigint")]
        public int UserID { get; set; }

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

        public void upcast(ArUserModels md)
        {
            Breviary = md.Breviary;
            Name = md.Name;
            Number = md.Number;
            Telephone = md.Telephone;
            Mobile = md.Mobile;
            Birthday = md.Birthday;
            EntryDate = md.EntryDate;
            Position = md.Position;
            PoliticalStatus = md.PoliticalStatus;
            Address = md.Address;
            Skill = md.Skill;
            Experience = md.Experience;
            Remark = md.Remark;
            Image = md.Image;
            UserID = md.UserID;
            ChangeTime = md.ChangeTime;
            Changer = md.Changer;
            CreateTime = md.CreateTime;
            Creator = md.Creator;
        }
    }
}