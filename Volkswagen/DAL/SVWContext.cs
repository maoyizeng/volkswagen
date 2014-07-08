
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

/*        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
 */
    }
}

/*
 * 
using Volkswagen.Models;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace Volkswagen.DAL
{
    public class SVWContext : DbContext
    {
        public DbSet<Course> Courses { get; set; }


        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            modelBuilder.Entity<Course>()
                .HasMany(c => c.Instructors).WithMany(i => i.Courses)
                .Map(t => t.MapLeftKey("CourseID")
                    .MapRightKey("InstructorID")
                    .ToTable("CourseInstructor"));
            modelBuilder.Entity<Department>().MapToStoredProcedures();
 
        }
    }
}

*/
