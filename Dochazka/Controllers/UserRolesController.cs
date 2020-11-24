using Dochazka.Areas.Identity.Data;
using Dochazka.Data;
using Dochazka.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dochazka.Controllers
{
    [Authorize(Roles = "ContactAdministrators")]
    public class UserRolesController : DI_BaseController
    {        
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<UserRolesController> _logger;
        public UserRolesController(
            ILogger<UserRolesController> logger,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context,
            IAuthorizationService authorizationService) : base(context, authorizationService, userManager)
        {
            _roleManager = roleManager;            
        }
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            var userRolesViewModel = new List<UserRolesViewModel>();
            foreach (ApplicationUser user in users)
            {
                UserRolesViewModel userRoleViewModel = await BuildUserRoleViewModel(user);
                userRolesViewModel.Add(userRoleViewModel);
            }
            return View(userRolesViewModel);
        }

        public async Task<IActionResult> Manage(string id)
        {
            ViewBag.id = id;
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id = {id} cannot be found";
                return NotFound();
            }
            ViewBag.UserName = user.UserName;
            var model = new List<ManageUserRolesViewModel>();
            foreach (var role in _roleManager.Roles)
            {
                var userRolesViewModel = new ManageUserRolesViewModel
                {
                    RoleId = role.Id,
                    RoleName = role.Name
                };
                if (await _userManager.IsInRoleAsync(user, role.Name))
                {
                    userRolesViewModel.Selected = true;
                }
                else
                {
                    userRolesViewModel.Selected = false;
                }
                model.Add(userRolesViewModel);
            }
            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> Manage(List<ManageUserRolesViewModel> model, string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id = {id} cannot be found";
                return NotFound();
            }
            var roles = await _userManager.GetRolesAsync(user);
            var result = await _userManager.RemoveFromRolesAsync(user, roles);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Cannot remove user existing roles");
                return View(model);
            }
            result = await _userManager.AddToRolesAsync(user, model.Where(x => x.Selected).Select(y => y.RoleName));
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Cannot add selected roles to user");
                return View(model);
            }
            return RedirectToAction("Index");
        }

        // GET: UserRoles/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            ViewBag.id = id;
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id = {id} cannot be found";
                return NotFound();
            }            
            var teamAssignments = await _context.Teams
                                .Include(t => t.PrimaryManager).Where(t => t.PrimaryManagerId == id)
                                .ToListAsync();            
            if (teamAssignments.Count > 0)
            {
                ViewBag.ErrorMessage = $"User with Id = {id} is assigned as manager for the team {teamAssignments.First().TeamName} and can't be deleted until unassigned.";
                return View(BuildUserRoleViewModel(user, false).Result);
            }

            return View(BuildUserRoleViewModel(user).Result);
        }

        // POST: UserRoles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id, UserRolesViewModel user)
        {
            var originalUser = await _userManager.FindByIdAsync(id);
            if ((id != user.Id) || (originalUser == null))
            {
                ViewBag.ErrorMessage = $"User with Id = {id} cannot be found or user Id mistmatch";
                return View(user);
            }

            try
            {
                var roles = await _userManager.GetRolesAsync(originalUser);
                var result = await _userManager.RemoveFromRolesAsync(originalUser, roles);
                if (!result.Succeeded)
                {                    
                    ViewBag.ErrorMessage = "Cannot remove user existing roles";
                    return View(user);
                }
                result = await _userManager.DeleteAsync(originalUser);
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                //Log the error (uncomment ex variable name and write a log.)
                return RedirectToAction(nameof(Delete), new { id = user.Id, concurrencyError = true });
            }
        }        

        private async Task<UserRolesViewModel> BuildUserRoleViewModel(ApplicationUser user, bool canBeDeleted = true)
        {
            var userRoleViewModel = new UserRolesViewModel();
            userRoleViewModel.Id = user.Id;
            userRoleViewModel.Email = user.Email;
            userRoleViewModel.FullName = user.FullName;
            userRoleViewModel.UserName = user.UserName;
            userRoleViewModel.Roles = new List<string>(await _userManager.GetRolesAsync(user));
            userRoleViewModel.ConcurrencyStamp = user.ConcurrencyStamp;
            userRoleViewModel.CanBeDeleted = canBeDeleted;
            return userRoleViewModel;
        }
    }
}
