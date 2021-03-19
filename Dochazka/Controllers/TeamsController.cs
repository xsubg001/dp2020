using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Dochazka.Data;
using Dochazka.Models;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using Dochazka.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;

namespace Dochazka.Controllers
{
    public class TeamsController : BaseController
    {
        private readonly ILogger<TeamsController> _logger;

        public TeamsController(
            ILogger<TeamsController> logger,
            ApplicationDbContext context,
            IAuthorizationService authorizationService,
            UserManager<ApplicationUser> userManager)
            : base(context, authorizationService, userManager)
        {
            _logger = logger;
        }

        // GET: Teams
        public async Task<IActionResult> Index(int? pageNumber)
        {
            var teams = _context.Teams.Include(t => t.PrimaryManager).AsNoTracking();                                                     
            
            //return View(await teams.ToListAsync());
            return View(await PaginatedListViewModel<TeamModel>.CreateAsync(teams, pageNumber ?? 1, CommonConstants.PAGE_SIZE));
        }

        // GET: Teams/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();                
            }

            var team = await _context.Teams
                .Include(t => t.PrimaryManager)
                .FirstOrDefaultAsync(m => m.TeamModelId == id);

            if (team == null)
            {
                return NotFound();
            }

            ViewBag.teamMembers = _userManager.Users.Where(u => u.Team.TeamModelId == id).ToList();            
            return View(team);
        }

        // GET: Teams/Create
        [Authorize(Roles = "TeamAdministratorRole")]
        public async Task<IActionResult> Create()
        {
            ViewData["PrimaryManagerId"] = new SelectList(await GetUnassignedManagersAsync(null), "Id", "UserName");
            return View();
        }

        // POST: Teams/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "TeamAdministratorRole")]
        public async Task<IActionResult> Create([Bind("TeamName,PrimaryManagerId")] TeamModel team)
        { 
            if (_context.Teams.AsNoTracking().Any(t => t.TeamName.ToLower() == team.TeamName.ToLower()))
            {
                ModelState.AddModelError(string.Empty, "Unable to save team with this Team Name. The team with the same team name already exists. "
                                                     + "Please give a different name to the new team.");
                await PopulateViewDataWithSelectedItems(team);
                return View(team);
            }
            else if (ModelState.IsValid)
            {
                _context.Add(team);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Team entry is not valid. Model validation failed.");
                await PopulateViewDataWithSelectedItems(team);
                return View(team);
            }            
        }

        // GET: Teams/Edit/5
        [Authorize(Roles = "TeamAdministratorRole")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var team = await _context.Teams.FindAsync(id);
            if (team == null)
            {
                return NotFound();
            }

            await PopulateViewDataWithSelectedItems(team);
            return View(team);
        }

        // POST: Teams/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "TeamAdministratorRole")]
        public async Task<IActionResult> Edit(int id, [Bind("TeamModelId,TeamName,PrimaryManagerId")] TeamModel team)
        {
            if (id != team.TeamModelId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(team);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TeamExists(team.TeamModelId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            await PopulateViewDataWithSelectedItems(team);
            return View(team);
        }

        // GET: Teams/Delete/5
        [Authorize(Roles = "TeamAdministratorRole")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var team = await _context.Teams
                .Include(t => t.PrimaryManager)
                .FirstOrDefaultAsync(m => m.TeamModelId == id);
            if (team == null)
            {
                return NotFound();
            }

            var teamMembers = await _userManager.Users.Where(u => u.Team.TeamModelId == id).ToListAsync();
            
            if (teamMembers.Count > 0)
            {
                ViewBag.ErrorMessage = $"There are still {teamMembers.Count} team members assigned to this team. The team must have no members before it can be deleted.";
                ViewBag.CanBeDeleted = false;
            }
            else
            {
                ViewBag.CanBeDeleted = true;
            }
            return View(team);
        }

        // POST: Teams/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "TeamAdministratorRole")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var team = await _context.Teams.FindAsync(id);
            _context.Teams.Remove(team);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        /// <summary>
        /// Helper method: Check if team with id already exists
        /// </summary>
        /// <param name="id"></param>
        /// <returns>bool</returns>

        private bool TeamExists(int id)
        {
            return _context.Teams.Any(e => e.TeamModelId == id);
        }


        /// <summary>
        /// Helper method: Returns list of managers, which have no team assigned yet.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Task<IList<ApplicationUser>></returns>
        private async Task<IList<ApplicationUser>> GetUnassignedManagersAsync(string? id)
        {
            var allManagers = await _userManager.GetUsersInRoleAsync("TeamManagerRole");
            var assignedManagerIds = await _context.Teams
                                .Include(t => t.PrimaryManager)
                                .Where(pm => !string.IsNullOrEmpty(pm.PrimaryManagerId))
                                .Select(pm => pm.PrimaryManagerId)
                                .ToListAsync();
            var result = allManagers.Where(m => !assignedManagerIds.Contains(m.Id)).ToList();
            if (id != null)
            {
                result.Add(await _userManager.FindByIdAsync(id));
            }
            return result;
        }

        /// <summary>
        /// Helper method: Populates VieData with team name information
        /// </summary>
        /// <param name="team"></param>
        private async Task PopulateViewDataWithSelectedItems(TeamModel team)
        {            
            var unassignedManagers = await GetUnassignedManagersAsync(team.PrimaryManagerId);
            ViewData["PrimaryManagerId"] = new SelectList(unassignedManagers, "Id", "UserName", team.PrimaryManagerId ?? unassignedManagers.FirstOrDefault().Id);
        }
    }
}
