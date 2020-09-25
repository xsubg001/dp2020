using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using IdentityTutorial.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using System.Text;
using Microsoft.AspNetCore.Authorization;

namespace IdentityTutorial.Controllers
{
    public class HomeController : Controller
    {
        // requires using Microsoft.Extensions.Configuration;
        private readonly IConfiguration Configuration;
        private readonly ILogger<HomeController> _logger;
        private readonly IServiceProvider ServiceProvider;

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration, IServiceProvider serviceProvider)
        {
            _logger = logger;
            Configuration = configuration;
            ServiceProvider = serviceProvider;
        }

        public IActionResult Index()
        {
            //var myKeyValue = Configuration["MyKey"];
            //var title = Configuration["Position:Title"];
            //var name = Configuration["Position:Name"];
            //var defaultLogLevel = Configuration["Logging:LogLevel:Default"];


            //return Content($"MyKey value: {myKeyValue} \n" +
            //               $"Title: {title} \n" +
            //               $"Name: {name} \n" +
            //               $"Default Log Level: {defaultLogLevel}");

            return View(Configuration);
        }

        [Authorize(Roles = "Administrator")]
        public IActionResult GetUsers()
        {
            
            var userManager = ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var users = userManager.Users.ToList();
            StringBuilder result = new StringBuilder();
            foreach (var user in users)
            {
                result.AppendLine(user.UserName);
            }
            ViewData["Result"] = result.ToString();
            ViewBag.Users = users;

            //Task<IdentityUser> checkAppUser = userManager.FindByEmailAsync(userEmail);
            //checkAppUser.Wait();

            //IdentityUser appUser = checkAppUser.Result;

            //if (checkAppUser.Result == null)
            //{
            //    IdentityUser newAppUser = new IdentityUser
            //    {
            //        Email = userEmail,
            //        UserName = userEmail
            //    };

            //    Task<IdentityResult> taskCreateAppUser = userManager.CreateAsync(newAppUser, userPwd);
            //    taskCreateAppUser.Wait();

            //    if (taskCreateAppUser.Result.Succeeded)
            //    {
            //        appUser = newAppUser;
            //    }
            //}

            //Task<IdentityResult> newUserRole = userManager.AddToRoleAsync(appUser, roleName);
            //newUserRole.Wait();

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
