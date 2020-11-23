using System;
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
    public class TeamsController : DI_BaseController
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
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Teams.Include(t => t.PrimaryManager);                                                     
            return View(await applicationDbContext.ToListAsync());
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
                .FirstOrDefaultAsync(m => m.TeamId == id);
            if (team == null)
            {
                return NotFound();
            }

            return View(team);
        }

        // GET: Teams/Create
        public IActionResult Create()
        {
            ViewData["PrimaryManagerId"] = new SelectList(GetUnassignedManagersAsync().Result, "Id", "UserName");
            return View();
        }

        // POST: Teams/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TeamID,TeamName,PrimaryManagerId")] Team team)
        { 
            if (_context.Teams.AsNoTracking().Any(t => t.TeamName.ToLower() == team.TeamName.ToLower()))
            {
                ModelState.AddModelError(string.Empty, "Unable to save team with this Team Name. The team with the same team name already exists. "
                                                     + "Please give a different name to the new team.");
                PopulateViewDataWithSelectedItems(team);
                return View(team);
            }
            if (ModelState.IsValid)
            {
                _context.Add(team);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["PrimaryManagerId"] = new SelectList(GetUnassignedManagersAsync().Result, "Id", "UserName", team.PrimaryManagerId);
            return View(team);
        }

        // GET: Teams/Edit/5
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
            ViewData["PrimaryManagerId"] = new SelectList(GetUnassignedManagersAsync().Result, "Id", "UserName", team.PrimaryManagerId);
            return View(team);
        }

        // POST: Teams/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TeamId,TeamName,PrimaryManagerId")] Team team)
        {
            if (id != team.TeamId)
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
                    if (!TeamExists(team.TeamId))
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
            ViewData["PrimaryManagerId"] = new SelectList(GetUnassignedManagersAsync().Result, "Id", "UserName", team.PrimaryManagerId);
            return View(team);
        }

        // GET: Teams/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var team = await _context.Teams
                .Include(t => t.PrimaryManager)
                .FirstOrDefaultAsync(m => m.TeamId == id);
            if (team == null)
            {
                return NotFound();
            }

            return View(team);
        }

        // POST: Teams/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var team = await _context.Teams.FindAsync(id);
            _context.Teams.Remove(team);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TeamExists(int id)
        {
            return _context.Teams.Any(e => e.TeamId == id);
        }

        /// <summary>
        /// Helper method: Populates VieData values
        /// </summary>
        /// <param name="presenceRecordV2"></param>
        private void PopulateViewDataWithSelectedItems(Team team)
        {            
            ViewData["TeamName"] = team.TeamName;
            ViewData["PrimaryManagerId"] = new SelectList(GetUnassignedManagersAsync().Result, "Id", "UserName", team.PrimaryManagerId);
        }

        /// <summary>
        /// Helper method: Returns list of managers, which have no team assigned yet.
        /// </summary>
        /// <returns></returns>
        private async Task<IList<ApplicationUser>> GetUnassignedManagersAsync()
        {
            var allManagers = _userManager.GetUsersInRoleAsync("ContactManagers").Result;
            var assignedManagerIds = await _context.Teams
                                .Include(t => t.PrimaryManager)
                                .Select(pm => pm.PrimaryManagerId)
                                .ToListAsync();
            return allManagers.Where(m => !assignedManagerIds.Contains(m.Id)).ToList();
        }
    }
}
