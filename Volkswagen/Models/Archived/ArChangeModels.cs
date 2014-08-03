using System;
using System.Web;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Volkswagen.Models
{
    // 用作 EquipmentController 操作的参数的模型。

    [Table("ArChanges")]
    // 表11 修改记录
    public class ArChangeModels
    {

        [Required]
        [Display(Name = "修改人")]
        [StringLength(10)]
        public string Changer { get; set; }

        [Required]
        [Display(Name = "修改时间")]
        public Nullable<System.DateTime> ChangeTime { get; set; }

        [Required]
        [Display(Name = "所属表")]
        [StringLength(30)]
        public string ChangeTable { get; set; }

        [Required]
        [Display(Name = "修改前记录")]
        [Column(TypeName = "ntext")]
        public string FormerRecord { get; set; }

        [Required]
        [Display(Name = "修改后记录")]
        [Column(TypeName = "ntext")]
        public string LaterRecord { get; set; }

        [Key]
        [Display(Name = "编号")]
        [Column(TypeName = "bigint")]
        public int UserID { get; set; }

    }
}