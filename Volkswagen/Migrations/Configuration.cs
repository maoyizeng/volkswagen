namespace Volkswagen.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using Volkswagen.Models;

    internal sealed class Configuration : DbMigrationsConfiguration<Volkswagen.DAL.SVWContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
            
        }

        protected override void Seed(Volkswagen.DAL.SVWContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //
            this.AddUserAndRoles();
        }

        bool AddUserAndRoles()
        {
            bool success = false;
            var idManager = new IdentityManager();
            success = idManager.CreateRole("Admin");
            if (!success == true) return success;
            success = idManager.CreateRole("CanEdit");
            if (!success == true) return success;
            success = idManager.CreateRole("User");
            if (!success) return success;
            var newUser = new ApplicationUser()
            {
                UserName = "jatten",


            };

            success = idManager.CreateUser(newUser, "Password1");
            if (!success) return success;
            success = idManager.AddUserToRole(newUser.Id, "Admin");
            if (!success) return success;
            success = idManager.AddUserToRole(newUser.Id, "CanEdit");
            if (!success) return success;
            success = idManager.AddUserToRole(newUser.Id, "User");
            if (!success) return success;

            return success;

        }
    }
}
