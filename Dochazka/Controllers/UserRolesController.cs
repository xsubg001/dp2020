using Dochazka.Areas.Identity.Data;
using Dochazka.Data;
using Dochazka.HelperClasses;
using Dochazka.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dochazka.Controllers
{    
    public class UserRolesController : BaseController
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
            _logger = logger;
        }
        public async Task<IActionResult> Index(string sortOrder, string currentFilter, string searchString, int? pageNumber)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["NameSortParm"] = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["TeamSortParm"] = sortOrder == "team" ? "team_desc" : "team";

            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewData["CurrentFilter"] = searchString;

            var users = await _userManager.Users.Include(u => u.Team).ToListAsync();
            var userRolesViewModel = new List<UserRolesViewModel>();
            foreach (ApplicationUser user in users)
            {
                UserRolesViewModel userRoleViewModel = await BuildUserRoleViewModel(user);
                userRolesViewModel.Add(userRoleViewModel);
            }

            if (!String.IsNullOrEmpty(searchString))
            {
                userRolesViewModel = userRolesViewModel.Where(u => u.FullName.ToLower().Contains(searchString.ToLower())).ToList();
            }

            switch (sortOrder)
            {
                case "name_desc":
                    userRolesViewModel = userRolesViewModel.OrderByDescending(u => u.FullName).ToList();
                    break;
                case "team":
                    userRolesViewModel = userRolesViewModel.OrderBy(u => u.TeamName).ToList();
                    break;
                case "team_desc":
                    userRolesViewModel = userRolesViewModel.OrderByDescending(u => u.TeamName).ToList();
                    break;
                default:
                    userRolesViewModel = userRolesViewModel.OrderBy(u => u.FullName).ToList();
                    break;
            }

            return View(PaginatedList<UserRolesViewModel>.Create(userRolesViewModel.AsQueryable(), pageNumber ?? 1, CommonConstants.PAGE_SIZE));            
        }

        [Authorize(Roles = "TeamAdministratorRole")]
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
                TeamModelId = user.Team.TeamModelId
            };

            ViewBag.Teams = new SelectList(await _context.Teams.ToListAsync(), "TeamModelId", "TeamName", model.TeamModelId ?? default(int));
            ViewBag.UserName = user.UserName;


            foreach (var role in _roleManager.Roles.ToList())
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
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "TeamAdministratorRole")]
        public async Task<IActionResult> Manage(ManageUserViewModel input)
        {
            var user = await _userManager.FindByIdAsync(input.UserId);
            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id = {input.UserId} cannot be found";
                return NotFound();
            }

            if (input.TeamModelId != null)
            {
                user.Team = await _context.Teams.FindAsync(input.TeamModelId);
            }
            else
            {
                user.Team = await _context.Teams.Where(t => t.TeamName == CommonConstants.DEFAULT_TEAM ).FirstOrDefaultAsync();
            }

            if (input.TeamModelId != user.Team?.TeamModelId)
            {                
                await _userManager.UpdateAsync(user);
            }

            var roles = await _userManager.GetRolesAsync(user);
            var result = await _userManager.RemoveFromRolesAsync(user, roles);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Failed to remove existing roles from the user");
                return View(input);
            }
            result = await _userManager.AddToRolesAsync(user, input.RoleSelections.Where(x => x.Selected).Select(y => y.RoleName));
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Failed to add selected roles to the user");
                return View(input);
            }
            return RedirectToAction("Index");
        }

        // GET: UserRoles/Delete/5
        [Authorize(Roles = "TeamAdministratorRole")]
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
        [Authorize(Roles = "TeamAdministratorRole")]
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
                    ViewBag.ErrorMessage = "Failed to remove existing roles from the user";
                    return View(user);
                }

                result = await _userManager.DeleteAsync(originalUser);                
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {                
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
