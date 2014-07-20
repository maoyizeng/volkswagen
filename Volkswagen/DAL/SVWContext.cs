
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

/*        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
 */
    }
}

