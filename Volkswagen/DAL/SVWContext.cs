
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Web;
using Volkswagen.Models;

namespace Volkswagen.DAL
{
    public class SVWContext : DbContext
    {
        public DbSet<EquipmentModels> Equipments { get; set; }

        public DbSet<InspectionModels> Inspections { get; set; }

        public DbSet<SpareModels> Spares { get; set; }

        public DbSet<SpareUserModels> SpareUsers { get; set; }

        public DbSet<SpareOrderModels> SpareOrders { get; set; }

        public DbSet<RepairModels> Repairs { get; set; }

        public DbSet<ShiftModels> Shifts { get; set; }

        public DbSet<EquipLogModels> EquipLogs { get; set; }

        public DbSet<MaintainModels> Maintains { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EquipLogModels>().Property(ov => ov.OriginValue).HasPrecision(18, 2);
            modelBuilder.Entity<EquipLogModels>().Property(d => d.Depreciation).HasPrecision(18, 2);
        }
 
   //     public DbSet<EquipmentModels> Equipments { get; set; }
    }
}

