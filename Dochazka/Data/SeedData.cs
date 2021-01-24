using Dochazka.Areas.Identity.Data;
using Dochazka.Data;
using Dochazka.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

// dotnet aspnet-codegenerator razorpage -m Contact -dc ApplicationDbContext -udl -outDir Pages\Contacts --referenceScriptLibraries
namespace ContactManager.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider, string testUserPw)
        {
            using (var context = new ApplicationDbContext(serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>()))
            {
                // For sample purposes seed both with the same password.
                // Password is set with the following:
                // dotnet user-secrets set SeedUserPW <pw>
                // The admin user can do anything

                foreach (var role in Enum.GetNames(typeof(Roles)))
                {
                    await CreateRole(serviceProvider, role);
                }

                var adminID = await EnsureUser(serviceProvider, testUserPw, "admin@contoso.com", "Gabriela", "Cimoradska");
                await EnsureRole(serviceProvider, adminID, Roles.TeamAdministratorRole.ToString());
                await EnsureRole(serviceProvider, adminID, Roles.TeamManagerRole.ToString());
                await EnsureRole(serviceProvider, adminID, Roles.TeamMemberRole.ToString());
                await InitDefaultTeam(serviceProvider, context, CommonConstants.DEFAULT_TEAM, adminID);                
            }
        }


        private static async Task<string> EnsureUser(IServiceProvider serviceProvider,
                                            string testUserPw, string userName, string firstName, string lastName)
        {
            var userManager = serviceProvider.GetService<UserManager<ApplicationUser>>();

            var user = await userManager.FindByNameAsync(userName);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = userName,
                    Email = userName,
                    FirstName = firstName,
                    LastName = lastName,
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(user, testUserPw);
            }

            if (user == null)
            {
                throw new Exception("The password is probably not strong enough!");
            }

            return user.Id;
        }

        private static async Task<IdentityResult> EnsureRole(IServiceProvider serviceProvider,
                                                              string uid, string role)
        {
            IdentityResult IR = null;
            var roleManager = serviceProvider.GetService<RoleManager<IdentityRole>>();

            if (roleManager == null)
            {
                throw new Exception("roleManager null");
            }

            if (!await roleManager.RoleExistsAsync(role))
            {
                IR = await roleManager.CreateAsync(new IdentityRole(role));
            }

            var userManager = serviceProvider.GetService<UserManager<ApplicationUser>>();

            var user = await userManager.FindByIdAsync(uid);

            if (user == null)
            {
                throw new Exception("The testUserPw password was probably not strong enough!");
            }

            IR = await userManager.AddToRoleAsync(user, role);

            return IR;
        }

        private static async Task<IdentityResult> CreateRole(IServiceProvider serviceProvider, string role)
        {
            IdentityResult IR = null;
            var roleManager = serviceProvider.GetService<RoleManager<IdentityRole>>();

            if (roleManager == null)
            {
                throw new Exception("roleManager null");
            }

            if (!await roleManager.RoleExistsAsync(role))
            {
                IR = await roleManager.CreateAsync(new IdentityRole(role));
            }

            return IR;
        }


        private static async Task<bool> InitDefaultTeam(IServiceProvider serviceProvider, ApplicationDbContext context, string teamName, string primaryManagerId)
        {
            var userManager = serviceProvider.GetService<UserManager<ApplicationUser>>();

            var user = await userManager.FindByIdAsync(primaryManagerId);
            if (!(context.Teams.Any(t => t.TeamName == teamName)))
            {
                var team = context.Teams.Add(
                    new Team
                    {
                        TeamName = teamName,
                        PrimaryManagerId = primaryManagerId                        
                    }
                );
                context.SaveChanges();
            }

            var defaultTeam = context.Teams.Where(t => t.TeamName == teamName && t.PrimaryManagerId == primaryManagerId).First();

            if (user.Team == null)
            {
                user.Team = defaultTeam;                
                await userManager.UpdateAsync(user);
            }
            return true;
        }        
    }
}
