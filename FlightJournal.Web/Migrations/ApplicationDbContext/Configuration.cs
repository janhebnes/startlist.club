using System.Security;
using FlightJournal.Web.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity.Migrations;
using System.Linq;

namespace FlightJournal.Web.Migrations.ApplicationDbContext
{

    internal sealed class Configuration : DbMigrationsConfiguration<FlightJournal.Web.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            MigrationsDirectory = @"Migrations\ApplicationDbContext";
        }

        protected override void Seed(FlightJournal.Web.Models.ApplicationDbContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  Only seed if the database is empty
            if (!context.Users.Any()
                && (!context.Roles.Any()))
            {
                InitializeDemoMemberships(context);
            }

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
        }

        //Create demo accounts and their roles
        public static void InitializeDemoMemberships(Models.ApplicationDbContext db)
        {
            foreach (var demoUsers in FlightJournal.Web.Configuration.Demo.GetDemoMembershipTemplates)
            {
                GenerateRoleAndDemoUser(db, demoUsers.RoleName, demoUsers.Name, demoUsers.Password, demoUsers.PhoneNumber);
            }
        }

        private static void GenerateRoleAndDemoUser(Models.ApplicationDbContext db, string roleName, string name, string password, string phoneNumber)
        {
            // remove this, because the OWIN context does not yet exist:
            ////var userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            ////var roleManager = HttpContext.Current.GetOwinContext().Get<ApplicationRoleManager>();
            // and replace it with:
            var userManager = new ApplicationUserManager(new UserStore<ApplicationUser>(db));
            var roleManager = new ApplicationRoleManager(new RoleStore<IdentityRole>(db));

            // because the context is created locally it must be configured with basic information to function
            userManager.UserValidator = new UserValidator<ApplicationUser>(userManager)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = true
            };
            // Configure validation logic for passwords
            userManager.PasswordValidator = new PasswordValidator
            {
                RequiredLength = 5,
                RequireNonLetterOrDigit = false,
                RequireDigit = false,
                RequireLowercase = false,
                RequireUppercase = false,
            };

            //Create Role if it does not exist
            var role = roleManager.FindByName(roleName);
            if (role == null && !string.IsNullOrEmpty(roleName))
            {
                role = new IdentityRole(roleName);
                var roleresult = roleManager.Create(role);
            }

            var user = userManager.FindByName(name);
            if (user == null)
            {
                // Allowing match on Mobil nr to be made on Pilot Binding
                user = new ApplicationUser
                {
                    UserName = name,
                    Email = name,
                    EmailConfirmed = true,
                    PhoneNumber = phoneNumber,
                    PhoneNumberConfirmed = true
                };
                var result = userManager.Create(user, password);
                if (!result.Succeeded)
                {
                    throw new SecurityException(string.Format("Failed to generate user {0} for ApplocationDbContext {1}", user, result.Errors.FirstOrDefault()));
                }
                result = userManager.SetLockoutEnabled(user.Id, false);
            }

            // Add user to Role if not already added
            var rolesForUser = userManager.GetRoles(user.Id);
            if (role != null 
                && !string.IsNullOrEmpty(roleName) 
                && !rolesForUser.Contains(role.Name))
            {
                var result = userManager.AddToRole(user.Id, role.Name);
            }
        }
    }
}
