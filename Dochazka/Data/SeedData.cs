using Dochazka.Areas.Identity.Data;
using Dochazka.Data;
using Dochazka.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ContactManager.Data
{
    public static class SeedData
    {
        /// <summary>
        /// Main initializing method which executes initialization steps
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="testUserPw"></param>
        /// <returns>void</returns>
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
                    await EnsureRole(serviceProvider, role);
                }

                var adminID = await EnsureUser(serviceProvider, testUserPw, "admin@contoso.com", "Gabriela", "Cimoradska");
                await EnsureUserIsInRole(serviceProvider, adminID, Roles.TeamAdministratorRole.ToString());
                await EnsureUserIsInRole(serviceProvider, adminID, Roles.TeamManagerRole.ToString());
                await EnsureUserIsInRole(serviceProvider, adminID, Roles.TeamMemberRole.ToString());
                await EnsureDefaultTeam(serviceProvider, context, CommonConstants.DEFAULT_TEAM, adminID);                
            }
        }

        /// <summary>
        /// Ensures the user with input params exists if it doesn't exist yet
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="testUserPw"></param>
        /// <param name="userName"></param>
        /// <param name="firstName"></param>
        /// <param name="lastName"></param>
        /// <returns>user.Id</returns>
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

        /// <summary>
        /// Ensures the user with uid is given the role if not yet
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="uid"></param>
        /// <param name="role"></param>
        /// <returns>IdentityResult</returns>
        private static async Task<IdentityResult> EnsureUserIsInRole(IServiceProvider serviceProvider,
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

        /// <summary>
        /// Creates role if it doesn't exist yet
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="role"></param>
        /// <returns>IdentityResult</returns>
        private static async Task<IdentityResult> EnsureRole(IServiceProvider serviceProvider, string role)
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

        /// <summary>
        /// Creates a default team and sets its PrimaryManager if it doesn't exist yet
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="context"></param>
        /// <param name="teamName"></param>
        /// <param name="primaryManagerId"></param>
        /// <returns>bool</returns>
        private static async Task<bool> EnsureDefaultTeam(IServiceProvider serviceProvider, ApplicationDbContext context, string teamName, string primaryManagerId)
        {
            var userManager = serviceProvider.GetService<UserManager<ApplicationUser>>();

            var user = await userManager.FindByIdAsync(primaryManagerId);
            if (!(context.Teams.Any(t => t.TeamName == teamName)))
            {
                var team = context.Teams.Add(
                    new TeamModel
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