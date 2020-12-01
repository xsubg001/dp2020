using Dochazka.Areas.Identity.Data;
using Dochazka.Data;
using Dochazka.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dochazka.Controllers
{
    [Authorize(Roles = "TeamAdministratorRole")]
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
            var users = await _userManager.Users.Include(u => u.Team).ToListAsync();
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
            var user = await _context.Users.Include(u => u.Team).FirstOrDefaultAsync(u => u.Id == id);            
            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id = {id} cannot be found";
                return NotFound();
            }
            var model = new ManageUserViewModel
            { 
                UserId = user.Id,
                TeamId = user.Team.TeamId
            };
            ViewBag.Teams = new SelectList(await _context.Teams.ToListAsync(), "TeamId", "TeamName", model.TeamId ?? default(int));
            ViewBag.UserName = user.UserName;


            foreach (var role in _roleManager.Roles)
            {
                var roleSelection = new RoleSelection
                {
                    RoleId = role.Id,
                    RoleName = role.Name
                };
                if (await _userManager.IsInRoleAsync(user, role.Name))
                {
                    roleSelection.Selected = true;
                }
                else
                {
                    roleSelection.Selected = false;
                }
                model.RoleSelections.Add(roleSelection);
            }            

            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> Manage(ManageUserViewModel input, string id)
        {
            var user = await _userManager.FindByIdAsync(input.UserId);
            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id = {input.UserId} cannot be found";
                return NotFound();
            }

            if (input.TeamId != null)
            {
                user.Team = await _context.Teams.FindAsync(input.TeamId);
            }
            else
            {
                user.Team = await _context.Teams.Where(t => t.TeamName == CommonConstants.DEFAULT_TEAM ).FirstOrDefaultAsync();
            }

            if (input.TeamId != user.Team?.TeamId)
            {                
                await _userManager.UpdateAsync(user);
            }

            var roles = await _userManager.GetRolesAsync(user);
            var result = await _userManager.RemoveFromRolesAsync(user, roles);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Cannot remove user existing roles");
                return View(input);
            }
            result = await _userManager.AddToRolesAsync(user, input.RoleSelections.Where(x => x.Selected).Select(y => y.RoleName));
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Cannot add selected roles to user");
                return View(input);
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
                return View(await BuildUserRoleViewModel(user, false));
            }

            return View(await BuildUserRoleViewModel(user));
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
            userRoleViewModel.TeamName = user.Team?.TeamName;
            userRoleViewModel.Roles = new List<string>(await _userManager.GetRolesAsync(user));
            userRoleViewModel.ConcurrencyStamp = user.ConcurrencyStamp;
            userRoleViewModel.CanBeDeleted = canBeDeleted;
            return userRoleViewModel;
        }
    }
}
