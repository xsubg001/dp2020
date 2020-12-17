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
using System.Collections.Generic;
using Dochazka.HelperClasses;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Razor;

namespace Dochazka.Controllers
{
    public class AttendanceRecordsController : DI_BaseController
    {
        private readonly ILogger<AttendanceRecordsController> _logger;

        public AttendanceRecordsController(
            ILogger<AttendanceRecordsController> logger,
            ApplicationDbContext context,
            IAuthorizationService authorizationService,
            UserManager<ApplicationUser> userManager)
            : base(context, authorizationService, userManager)
        {
            _logger = logger;
        }

        // GET: AttendanceRecords
        public async Task<IActionResult> Index(string sortOrder, string currentFilter, string searchString, int? pageNumber, string infoMessage)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["DateSortParm"] = string.IsNullOrEmpty(sortOrder) ? "date" : "";
            ViewData["NameSortParm"] = sortOrder == "name" ? "name_desc" : "name";
            ViewData["ApprovalStatusSortParm"] = sortOrder == "approval" ? "approval_desc" : "approval";
            ViewData["InfoMessage"] = infoMessage;


            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewData["CurrentFilter"] = searchString;

            var currentUserId = _userManager.GetUserId(User);
            var attendanceRecords = _context.AttendanceRecords.Include(p => p.Employee).ThenInclude(e => e.Team).AsNoTracking();

            if (await _userManager.IsInRoleAsync(await _userManager.FindByIdAsync(currentUserId), Roles.TeamAdministratorRole.ToString()))
            {                
            }
            else if (await _userManager.IsInRoleAsync(await _userManager.FindByIdAsync(currentUserId), Roles.TeamManagerRole.ToString()))
            {
                attendanceRecords = attendanceRecords.Where(ar => ar.EmployeeId == currentUserId || ar.Employee.Team.PrimaryManagerId == currentUserId);
            }
            else
            {
                attendanceRecords = attendanceRecords.Where(ar => ar.EmployeeId == currentUserId);
            }            

            if (!String.IsNullOrEmpty(searchString))
            {
                //attendanceRecords = attendanceRecords.AsEnumerable().Where(ar => (ar.Employee.LastName + ar.Employee.FirstName).Contains(searchString)).AsQueryable();
                attendanceRecords = attendanceRecords.Where(ar => (ar.Employee.LastName + ar.Employee.FirstName).Contains(searchString));
            }

            switch (sortOrder)
            {
                case "name_desc":
                    attendanceRecords = attendanceRecords.OrderByDescending(ar => ar.Employee.LastName + ar.Employee.FirstName);
                    break;
                case "name":                    
                    attendanceRecords = attendanceRecords.OrderBy(ar => ar.Employee.LastName + ar.Employee.FirstName);
                    break;
                case "approval_desc":
                    attendanceRecords = attendanceRecords.OrderByDescending(ar => ar.ManagerApprovalStatus);
                    break;
                case "approval":
                    attendanceRecords = attendanceRecords.OrderBy(ar => ar.ManagerApprovalStatus);
                    break;
                case "date":                    
                    attendanceRecords = attendanceRecords.OrderBy(ar => ar.WorkDay);
                    break;
                default:
                    attendanceRecords = attendanceRecords.OrderByDescending(ar => ar.WorkDay);
                    break;
            }
            ViewData["ManagerApprovalStatusDisabled"] = false;
            return View(await PaginatedList<AttendanceRecord>.CreateAsync(attendanceRecords, pageNumber ?? 1, CommonConstants.PAGE_SIZE));
        }

        // POST: AttendanceRecords/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        //public async Task<IActionResult> Index(string[] workDay, string[] employeeId, string[] managerApprovalStatus)
        public async Task<IActionResult> Index(BulkApprovalViewModel bulkApprovals)
        {
            string infoMessage = "";
            int successUpdates = 0;
            for (int i = 0; i < bulkApprovals.EmployeeIds.Count(); i++)
            {
                var employeeId = bulkApprovals.EmployeeIds[i];
                var workday = bulkApprovals.WorkDays[i];
                var newManagerApprovalStatus = bulkApprovals.ManagerApprovalStatuses[i];

                if ((employeeId == null) || (workday == null))
                {
                    return NotFound();
                }
                var attendanceRecord = await _context.AttendanceRecords.FindAsync(employeeId, workday);
                if (attendanceRecord == null)
                {
                    return NotFound();
                }

                if (attendanceRecord.ManagerApprovalStatus != newManagerApprovalStatus)
                {
                    attendanceRecord.ManagerApprovalStatus = newManagerApprovalStatus;
                    if (ModelState.IsValid)
                    {
                        try
                        {
                            _context.Update(attendanceRecord);
                            await _context.SaveChangesAsync();
                            ++successUpdates;                            
                        }
                        catch (DbUpdateConcurrencyException)
                        {
                            if (!AttendanceRecordExists(attendanceRecord.EmployeeId, attendanceRecord.WorkDay))
                            {
                                return NotFound();
                            }
                            else
                            {
                                throw;
                            }
                        }
                    }
                }
            }
            if (successUpdates == 1)
            {
                infoMessage = $"Approval statuses updated successfully for {successUpdates} record";
            }
            else if (successUpdates > 1)
            {
                infoMessage = $"Approval statuses updated successfully for {successUpdates} records";
            }
            return RedirectToAction(nameof(Index), new { infoMessage = infoMessage});
        }

            // GET: AttendanceRecords/Details/5
            public async Task<IActionResult> Details(string employeeId, DateTime workday)
        {
            if ((employeeId == null) || (workday == null))
            {
                return NotFound();
            }

            var attendanceRecord = await _context.AttendanceRecords.Include(p => p.Employee)    
                                                                        .ThenInclude(e => e.Team)
                                                                            .ThenInclude(t => t.PrimaryManager)                                                                        
                                                                   .FirstOrDefaultAsync(p => p.EmployeeId == employeeId && p.WorkDay == workday);

            //ViewData["employeeTeam"]  = await _context.Teams.FindAsync(attendanceRecord.Employee.TeamId);

            if (attendanceRecord == null)
            {
                return NotFound();
            }

            return View(attendanceRecord);
        }

        // GET: AttendanceRecords/Create
        public IActionResult Create()
        {            
            ViewData["EmployeeId"] = new SelectList(_context.Users, "Id", "UserName");
            ViewData["MorningAttendance"] = new SelectList(Enum.GetNames(typeof(Attendance)));
            ViewData["AfternoonAttendance"] = new SelectList(Enum.GetNames(typeof(Attendance)));
            return View(new AttendanceRecord());            
        }

        // POST: AttendanceRecords/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("WorkDay,MorningAttendance,AfternoonAttendance")] AttendanceRecord attendanceRecord)
        {
            attendanceRecord.EmployeeId = User.FindFirstValue(ClaimTypes.NameIdentifier);            
            if (_context.AttendanceRecords.AsNoTracking().Any(p => p.EmployeeId == attendanceRecord.EmployeeId && p.WorkDay == attendanceRecord.WorkDay))
            {
                ModelState.AddModelError(string.Empty, "Unable to save this Presence Record. The entry with this work day date already exists. "
                                                     + "Please select a different date or remove the original entry with the same date first.");
                PopulateViewDataWithSelectedItems(attendanceRecord);
                return View(attendanceRecord);
            }

            if (ModelState.IsValid)
            {
                _context.Add(attendanceRecord);                
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            PopulateViewDataWithSelectedItems(attendanceRecord);
            return View(attendanceRecord);
        }

        // GET: AttendanceRecords/Edit/5
        public async Task<IActionResult> Edit(string employeeId, DateTime workday)
        {
            if ((employeeId == null) || (workday == null))
            {
                return NotFound();
            }

            var attendanceRecord = await _context.AttendanceRecords.Include(p => p.Employee).ThenInclude(e => e.Team)
                                                                    .AsNoTracking().FirstOrDefaultAsync(p => p.EmployeeId == employeeId && p.WorkDay == workday);
            if (attendanceRecord == null)
            {
                return NotFound();
            }

            PopulateViewDataWithSelectedItems(attendanceRecord);

            var currentUserId = _userManager.GetUserId(User);
            if ((attendanceRecord.Employee.Team.PrimaryManagerId == currentUserId) 
                || (await _userManager.IsInRoleAsync(await _userManager.FindByIdAsync(currentUserId), Roles.TeamAdministratorRole.ToString())))
            {
                ViewData["ManagerApprovalStatusDisabled"] = false;
            }
            
            return View(attendanceRecord);
        }


        // POST: AttendanceRecords/Edit/5
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

            var attendanceRecord = await _context.AttendanceRecords.FindAsync(employeeId, workday);
            if (attendanceRecord == null)
            {
                return NotFound();
            }

            if (await TryUpdateModelAsync<AttendanceRecord>(
                attendanceRecord,
                "",
                s => s.MorningAttendance, s => s.AfternoonAttendance, s => s.ManagerApprovalStatus))
            {
                if (ModelState.IsValid)
                {
                    try
                    {
                        _context.Update(attendanceRecord);
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!AttendanceRecordExists(attendanceRecord.EmployeeId, attendanceRecord.WorkDay))
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
            
            PopulateViewDataWithSelectedItems(attendanceRecord);
            return View(attendanceRecord);
        }

        // GET: AttendanceRecords/Delete/5
        public async Task<IActionResult> Delete(string employeeId, DateTime workday)
        {
            if ((employeeId == null) || (workday == null))
            {
                return NotFound();
            }

            var attendanceRecord = await _context.AttendanceRecords.Include(p => p.Employee).AsNoTracking().FirstOrDefaultAsync(p => p.EmployeeId == employeeId && p.WorkDay == workday);
            if (attendanceRecord == null)
            {
                return NotFound();
            }

            return View(attendanceRecord);
        }

        // POST: AttendanceRecords/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string employeeId, DateTime workday)
        {            
            _logger.LogInformation("Deleteting item with empoloyeeId={employeeId}, workday={workday}", employeeId, workday.ToShortDateString());
            if ((employeeId == null) || (workday == null))
            {
                return NotFound();
            }
            var originalAttendanceRecord = await _context.AttendanceRecords.FindAsync(employeeId, workday);

            _context.AttendanceRecords.Remove(originalAttendanceRecord);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Helper method: Checks if any presence record with the same userId and workday already exists
        /// </summary>
        /// <param name="id"></param>
        /// <param name="workday"></param>
        /// <returns></returns>
        private bool AttendanceRecordExists(string id, DateTime workday)
        {
            return _context.AttendanceRecords.Any(e => e.EmployeeId == id && e.WorkDay == workday);
        }


        /// <summary>
        /// Helper method: Prepares ViewData and pre-selects last selection
        /// </summary>
        /// <param name="attendanceRecord"></param>
        private void PopulateViewDataWithSelectedItems(AttendanceRecord attendanceRecord)
        {
            ViewData["EmployeeId"] = new SelectList(_context.Users, "Id", "UserName", attendanceRecord.EmployeeId);
            ViewData["MorningAttendance"] = new SelectList(Enum.GetNames(typeof(Attendance)), attendanceRecord.MorningAttendance);
            ViewData["AfternoonAttendance"] = new SelectList(Enum.GetNames(typeof(Attendance)), attendanceRecord.AfternoonAttendance);
            ViewData["ManagerApprovalStatus"] = new SelectList(Enum.GetNames(typeof(ManagerApprovalStatus)), attendanceRecord.ManagerApprovalStatus);
            ViewData["ManagerApprovalStatusDisabled"] = true;
        }
    }
}
