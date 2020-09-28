using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Dochazka.Data;
using Dochazka.Models;

namespace Dochazka.Controllers
{
    public class PresenceRecordsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PresenceRecordsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: PresenceRecords
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.PresenceRecords.Include(p => p.Employee);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: PresenceRecords/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var presenceRecord = await _context.PresenceRecords
                .Include(p => p.Employee)
                .FirstOrDefaultAsync(m => m.employeeId == id);
            if (presenceRecord == null)
            {
                return NotFound();
            }

            return View(presenceRecord);
        }

        // GET: PresenceRecords/Create
        public IActionResult Create()
        {
            ViewData["employeeId"] = new SelectList(_context.Users, "Id", "UserName");
            ViewData["dayTimeSlot"] = new SelectList(Enum.GetNames(typeof(DayTimeSlot)));
            ViewData["presence"] = new SelectList(Enum.GetNames(typeof(PresenceState)));
            return View();
        }

        // POST: PresenceRecords/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("WorkDay,DayTimeSlot,employeeId,Presence,ManagerApprovalStatus,RowVersion")] PresenceRecord presenceRecord)
        {
            if (ModelState.IsValid)
            {
                _context.Add(presenceRecord);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["employeeId"] = new SelectList(_context.Users, "Id", "Id", presenceRecord.employeeId);
            return View(presenceRecord);
        }

        // GET: PresenceRecords/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var presenceRecord = await _context.PresenceRecords.FindAsync(id);
            if (presenceRecord == null)
            {
                return NotFound();
            }
            ViewData["employeeId"] = new SelectList(_context.Users, "Id", "Id", presenceRecord.employeeId);
            return View(presenceRecord);
        }

        // POST: PresenceRecords/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("WorkDay,DayTimeSlot,employeeId,Presence,ManagerApprovalStatus,RowVersion")] PresenceRecord presenceRecord)
        {
            if (id != presenceRecord.employeeId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(presenceRecord);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PresenceRecordExists(presenceRecord.employeeId))
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
            ViewData["employeeId"] = new SelectList(_context.Users, "Id", "Id", presenceRecord.employeeId);
            return View(presenceRecord);
        }

        // GET: PresenceRecords/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var presenceRecord = await _context.PresenceRecords
                .Include(p => p.Employee)
                .FirstOrDefaultAsync(m => m.employeeId == id);
            if (presenceRecord == null)
            {
                return NotFound();
            }

            return View(presenceRecord);
        }

        // POST: PresenceRecords/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var presenceRecord = await _context.PresenceRecords.FindAsync(id);
            _context.PresenceRecords.Remove(presenceRecord);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PresenceRecordExists(string id)
        {
            return _context.PresenceRecords.Any(e => e.employeeId == id);
        }
    }
}
