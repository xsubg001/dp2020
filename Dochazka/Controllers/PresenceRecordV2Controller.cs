using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Dochazka.Data;
using Dochazka.Models;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Dochazka.Areas.Identity.Data;

namespace Dochazka.Controllers
{
    public class PresenceRecordV2Controller : DI_BaseController
    {
        private readonly ILogger<PresenceRecordV2Controller> _logger;

        public PresenceRecordV2Controller(
            ILogger<PresenceRecordV2Controller> logger,
            ApplicationDbContext context,
            IAuthorizationService authorizationService,
            UserManager<ApplicationUser> userManager)
            : base(context, authorizationService, userManager)
        {
            _logger = logger;
        }

        // GET: PresenceRecordV2
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.PresenceRecordsV2.Include(p => p.Employee);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: PresenceRecordV2/Details/5
        public async Task<IActionResult> Details(string employeeId, DateTime workday)
        {
            if ((employeeId == null) || (workday == null))
            {
                return NotFound();
            }

            var presenceRecordV2 = await _context.PresenceRecordsV2.Include(p => p.Employee)    
                                                                        .ThenInclude(e => e.Team)
                                                                            .ThenInclude(t => t.PrimaryManager)
                                                                   .FirstOrDefaultAsync(p => p.EmployeeId == employeeId && p.WorkDay == workday);

            //ViewData["employeeTeam"]  = await _context.Teams.FindAsync(presenceRecordV2.Employee.TeamId);

            if (presenceRecordV2 == null)
            {
                return NotFound();
            }

            return View(presenceRecordV2);
        }

        // GET: PresenceRecordV2/Create
        public IActionResult Create()
        {            
            ViewData["EmployeeId"] = new SelectList(_context.Users, "Id", "UserName");
            ViewData["MorningPresence"] = new SelectList(Enum.GetNames(typeof(PresenceState)));
            ViewData["AfternoonPresence"] = new SelectList(Enum.GetNames(typeof(PresenceState)));
            return View(new PresenceRecordV2());            
        }

        // POST: PresenceRecordV2/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("WorkDay,MorningPresence,AfternoonPresence")] PresenceRecordV2 presenceRecordV2)
        {
            presenceRecordV2.EmployeeId = User.FindFirstValue(ClaimTypes.NameIdentifier);            
            if (_context.PresenceRecordsV2.AsNoTracking().Any(p => p.EmployeeId == presenceRecordV2.EmployeeId && p.WorkDay == presenceRecordV2.WorkDay))
            {
                ModelState.AddModelError(string.Empty, "Unable to save this Presence Record. The entry with this work day date already exists. "
                                                     + "Please select a different date or remove the original entry with the same date first.");
                PopulateViewDataWithSelectedItems(presenceRecordV2);
                return View(presenceRecordV2);
            }

            if (ModelState.IsValid)
            {
                _context.Add(presenceRecordV2);                
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            PopulateViewDataWithSelectedItems(presenceRecordV2);
            return View(presenceRecordV2);
        }

        // GET: PresenceRecordV2/Edit/5
        public async Task<IActionResult> Edit(string employeeId, DateTime workday)
        {
            if ((employeeId == null) || (workday == null))
            {
                return NotFound();
            }

            var presenceRecordV2 = await _context.PresenceRecordsV2.Include(p => p.Employee).AsNoTracking().FirstOrDefaultAsync(p => p.EmployeeId == employeeId && p.WorkDay == workday);
            if (presenceRecordV2 == null)
            {
                return NotFound();
            }
            PopulateViewDataWithSelectedItems(presenceRecordV2);
            return View(presenceRecordV2);
        }

        /// <summary>
        /// Prepares ViewData and pre-selects last selection
        /// </summary>
        /// <param name="presenceRecordV2"></param>
        private void PopulateViewDataWithSelectedItems(PresenceRecordV2 presenceRecordV2)
        {
            ViewData["EmployeeId"] = new SelectList(_context.Users, "Id", "UserName", presenceRecordV2.EmployeeId);
            ViewData["MorningPresence"] = new SelectList(Enum.GetNames(typeof(PresenceState)), presenceRecordV2.MorningPresence);
            ViewData["AfternoonPresence"] = new SelectList(Enum.GetNames(typeof(PresenceState)), presenceRecordV2.AfternoonPresence);
        }

        // POST: PresenceRecordV2/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]        
        public async Task<IActionResult> Edit(string employeeId, DateTime workday, byte[] rowVersion)
        {
            if ((employeeId == null) || (workday == null))
            {
                return NotFound();
            }

            var presenceRecordV2 = await _context.PresenceRecordsV2.FindAsync(employeeId, workday);
            if (presenceRecordV2 == null)
            {
                return NotFound();
            }

            if (await TryUpdateModelAsync<PresenceRecordV2>(
                presenceRecordV2,
                "",
                s => s.MorningPresence, s => s.AfternoonPresence))
            {
                if (ModelState.IsValid)
                {
                    try
                    {
                        _context.Update(presenceRecordV2);
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!PresenceRecordV2Exists(presenceRecordV2.EmployeeId, presenceRecordV2.WorkDay))
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
            }
            
            PopulateViewDataWithSelectedItems(presenceRecordV2);
            return View(presenceRecordV2);
        }

        // GET: PresenceRecordV2/Delete/5
        public async Task<IActionResult> Delete(string employeeId, DateTime workday)
        {
            if ((employeeId == null) || (workday == null))
            {
                return NotFound();
            }

            var presenceRecordV2 = await _context.PresenceRecordsV2.Include(p => p.Employee).AsNoTracking().FirstOrDefaultAsync(p => p.EmployeeId == employeeId && p.WorkDay == workday);
            if (presenceRecordV2 == null)
            {
                return NotFound();
            }

            return View(presenceRecordV2);
        }

        // POST: PresenceRecordV2/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string employeeId, DateTime workday)
        {            
            _logger.LogInformation("Deleteting item with empoloyeeId={employeeId}, workday={workday}", employeeId, workday.ToShortDateString());
            if ((employeeId == null) || (workday == null))
            {
                return NotFound();
            }
            var originalPresenceRecordV2 = await _context.PresenceRecordsV2.FindAsync(employeeId, workday);

            _context.PresenceRecordsV2.Remove(originalPresenceRecordV2);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PresenceRecordV2Exists(string id, DateTime workday)
        {
            return _context.PresenceRecordsV2.Any(e => e.EmployeeId == id && e.WorkDay == workday);
        }
    }
}
