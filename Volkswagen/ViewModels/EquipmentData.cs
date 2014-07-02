using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Volkswagen.ViewModels
{
    public class EquipmentData
    {
        public String EquipmentNumber { get; set; }
        public String EquipmentName { get; set; }
        public String Server { get; set; }
        public int Section { get; set; }
        public int Area { get; set; }
        public String Pictures { get; set; }
        public int CheckCard { get; set; }
        public int MaintainCard { get; set; }
    }
}