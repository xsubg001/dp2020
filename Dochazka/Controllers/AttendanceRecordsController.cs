using System;
using System.Linq;
using System.Data;
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
using System.Globalization;
using Dochazka.HelperClasses;

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
        public async Task<IActionResult> Index(string sortOrder, string currentFilter, string searchString,
            int? pageNumber, string infoMessage, DateTime selectedMonth, bool getAsCsv)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["DateSortParm"] = string.IsNullOrEmpty(sortOrder) ? "date" : "";
            ViewData["NameSortParm"] = sortOrder == "name" ? "name_desc" : "name";
            ViewData["ApprovalStatusSortParm"] = sortOrder == "approval" ? "approval_desc" : "approval";
            ViewData["ManagerApprovalControlDisabled"] = true;
            ViewData["InfoMessage"] = infoMessage;

            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            _logger.LogInformation($"Request month value: {selectedMonth}");
            if ((selectedMonth == null) || (selectedMonth == DateTime.MinValue))
            {
                selectedMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            }

            ViewData["SelectedMonth"] = $"{selectedMonth.Year}-{selectedMonth.Month}";
            var daysInMonth = DateTime.DaysInMonth(selectedMonth.Year, selectedMonth.Month);

            ViewData["CurrentFilter"] = searchString;

            var currentUserId = _userManager.GetUserId(User);
            var attendanceRecords = _context.AttendanceRecords.Where(ar =>
                    ar.WorkDay >= selectedMonth && ar.WorkDay < selectedMonth.AddDays(daysInMonth))
                .Include(p => p.Employee)
                .ThenInclude(e => e.Team)
                .AsNoTracking();

            if (await _userManager.IsInRoleAsync(await _userManager.FindByIdAsync(currentUserId),
                Roles.TeamAdministratorRole.ToString()))
            {
            }
            else if (await _userManager.IsInRoleAsync(await _userManager.FindByIdAsync(currentUserId),
                Roles.TeamManagerRole.ToString()))
            {
                attendanceRecords = attendanceRecords.Where(ar =>
                    ar.EmployeeId == currentUserId || ar.Employee.Team.PrimaryManagerId == currentUserId);
            }
            else
            {
                attendanceRecords = attendanceRecords.Where(ar => ar.EmployeeId == currentUserId);
            }

            if (!String.IsNullOrEmpty(searchString))
            {
                attendanceRecords = attendanceRecords.Where(ar =>
                    (ar.Employee.LastName + ar.Employee.FirstName).Contains(searchString));
            }

            switch (sortOrder)
            {
                case "name_desc":
                    attendanceRecords =
                        attendanceRecords.OrderByDescending(ar => ar.Employee.LastName + ar.Employee.FirstName);
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

            if (getAsCsv)
            {
                DataTable exportTable = await GetAttendanceRecordsAsDataTable(attendanceRecords);
                var csvResult = new CSVResult(exportTable,
                    $"{currentUserId}_{DateTime.Now.ToString(CultureInfo.InvariantCulture)}.csv");
                return csvResult;
            }
            else
            {
                return View(await PaginatedList<AttendanceRecord>.CreateAsync(attendanceRecords, pageNumber ?? 1,
                    CommonConstants.PAGE_SIZE));
            }
        }

        private async Task<DataTable> GetAttendanceRecordsAsDataTable(IQueryable<AttendanceRecord> attendanceRecords)
        {
            var attendanceRecordsAsList = await attendanceRecords.ToListAsync();
            DataTable table = new DataTable("ExportAsCsv");
            DataColumn[] columns =
            {
                new DataColumn("WorkDay", typeof(String)),
                new DataColumn("MorningAttendance", typeof(String)),
                new DataColumn("AfternoonAttendance", typeof(String)),
                new DataColumn("FullName", typeof(String)),
                new DataColumn("UserName", typeof(String)),
                new DataColumn("Manager Approval Status", typeof(String))
            };

            table.Columns.AddRange(columns);

            foreach (var ar in attendanceRecordsAsList)
            {
                table.Rows.Add(new Object[]
                {
                    ar.WorkDay.Date.ToShortDateString(),
                    ar.MorningAttendance,
                    ar.AfternoonAttendance,
                    ar.Employee.FullName,
                    ar.Employee.UserName,
                    ar.ManagerApprovalStatus
                });
            }

            return table;
        }


        private async Task<DataTable> GetSummaryResultsAsDataTable(
            Dictionary<string, Dictionary<string, int>> byEmployeeIDResults, DateTime selectedMonth)
        {
            DataTable table = new DataTable("ExportAsCsv");
            DataColumn[] columns =
            {
                new DataColumn("employeeid", typeof(String)),
                new DataColumn("username", typeof(String)),
                new DataColumn("month", typeof(DateTime))
            };
            table.Columns.AddRange(columns);
            foreach (string attendanceValue in Enum.GetNames(typeof(Attendance)))
            {
                table.Columns.Add(new DataColumn(attendanceValue.ToLower(), typeof(int)));
            }

            foreach (var employeeID in byEmployeeIDResults.Keys)
            {
                DataRow row = table.NewRow();
                row["employeeid"] = employeeID;
                row["username"] = await _userManager.FindByIdAsync(employeeID);
                row["month"] = selectedMonth;
                foreach (string attendanceValue in byEmployeeIDResults[employeeID].Keys)
                {
                    row[attendanceValue] = byEmployeeIDResults[employeeID][attendanceValue];
                }

                table.Rows.Add(row);
            }

            return table;
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
            for (int i = 0; i < bulkApprovals.EmployeeIds?.Count(); i++)
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

            return RedirectToAction(nameof(Index), new {infoMessage = infoMessage});
        }


        // GET: Payroll summary for selected employees
        public async Task<IActionResult> PayrollSummary(string searchString, DateTime selectedMonth, bool getAsCsv)
        {
            _logger.LogInformation($"Request month value: {selectedMonth}");
            if ((selectedMonth == null) || (selectedMonth == DateTime.MinValue))
            {
                selectedMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            }

            ViewData["SelectedMonth"] = $"{selectedMonth.Year}-{selectedMonth.Month}";
            var daysInMonth = DateTime.DaysInMonth(selectedMonth.Year, selectedMonth.Month);

            ViewData["CurrentFilter"] = searchString;

            var currentUserId = _userManager.GetUserId(User);
            var attendanceRecords = _context.AttendanceRecords.Where(ar =>
                    ar.WorkDay >= selectedMonth && ar.WorkDay < selectedMonth.AddDays(daysInMonth))
                .Where(ar => ar.ManagerApprovalStatus == ManagerApprovalStatus.Approved)
                .Include(p => p.Employee)
                .ThenInclude(e => e.Team)
                .AsNoTracking();

            if (await _userManager.IsInRoleAsync(await _userManager.FindByIdAsync(currentUserId),
                Roles.TeamAdministratorRole.ToString()))
            {
            }
            else if (await _userManager.IsInRoleAsync(await _userManager.FindByIdAsync(currentUserId),
                Roles.TeamManagerRole.ToString()))
            {
                attendanceRecords = attendanceRecords.Where(ar =>
                    ar.EmployeeId == currentUserId || ar.Employee.Team.PrimaryManagerId == currentUserId);
            }
            else
            {
                attendanceRecords = attendanceRecords.Where(ar => ar.EmployeeId == currentUserId);
            }

            if (!String.IsNullOrEmpty(searchString))
            {
                attendanceRecords = attendanceRecords.Where(ar =>
                    (ar.Employee.LastName + ar.Employee.FirstName).Contains(searchString));
            }

            var byEmployeeIDResults = new Dictionary<string, Dictionary<string, int>>();
            var arByUsername = attendanceRecords.ToList().GroupBy(ar => ar.EmployeeId);

            foreach (var group in arByUsername)
            {
                var afternoonSummary = group.GroupBy(ar => ar.AfternoonAttendance.ToString(),
                    ar => ar.AfternoonAttendance,
                    (afternoon, allAfternoons) => new {key = afternoon, countAR = allAfternoons.Count()});
                var morningSummary = group.GroupBy(ar => ar.MorningAttendance.ToString(), ar => ar.MorningAttendance,
                    (morning, allMornings) => new {key = morning, countAR = allMornings.Count()});
                byEmployeeIDResults[group.Key] = new Dictionary<string, int>();
                foreach (var attendance in Enum.GetNames(typeof(Attendance)))
                {
                    byEmployeeIDResults[group.Key].Add(attendance, 0);
                    byEmployeeIDResults[group.Key][attendance] += afternoonSummary.Any(x => x.key == attendance)
                        ? afternoonSummary.First(x => x.key == attendance).countAR
                        : 0;
                    byEmployeeIDResults[group.Key][attendance] += morningSummary.Any(x => x.key == attendance)
                        ? morningSummary.First(x => x.key == attendance).countAR
                        : 0;
                }
            }

            DataTable exportTable = await GetSummaryResultsAsDataTable(byEmployeeIDResults, selectedMonth);

            if (getAsCsv)
            {
                var csvResult = new CSVResult(exportTable, $"{currentUserId}_{DateTime.Now.ToString(CultureInfo.InvariantCulture)}.csv");
                return csvResult;
            }
            else
            {
                List<PayrollSummaryModel> payrollSummaryList = ConvertDataTableToPayrollSummaryList(exportTable);
                //return View(await PaginatedList<PayrollSummaryModel>.Create(payrollSummaryList.AsQueryable<PayrollSummaryModel>(), pageNumber ?? 1, CommonConstants.PAGE_SIZE));
                return View(payrollSummaryList);
            }
        }

        private List<PayrollSummaryModel> ConvertDataTableToPayrollSummaryList(DataTable exportTable)
        {
            return exportTable.AsEnumerable().Select(m => new PayrollSummaryModel()
            {
                EmployeeID = m.Field<string>("EmployeeID".ToLower()),
                UserName = m.Field<string>("UserName".ToLower()),
                Month = m.Field<DateTime>("Month".ToLower()),
                Absence = m.Field<int>("Absence".ToLower()),
                DoctorSickness = m.Field<int>("DoctorSickness".ToLower()),
                PaidVacation = m.Field<int>("PaidVacation".ToLower()),
                LegalJustification = m.Field<int>("LegalJustification".ToLower()),
                Sickleave = m.Field<int>("Sickleave".ToLower()),
                UnpaidVacation = m.Field<int>("Sickleave".ToLower()),
                WorkingTime = m.Field<int>("WorkingTime".ToLower())
            }).ToList();
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
        public async Task<IActionResult> Create([Bind("WorkDay,MorningAttendance,AfternoonAttendance")]
            AttendanceRecord attendanceRecord)
        {
            attendanceRecord.EmployeeId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (_context.AttendanceRecords.AsNoTracking().Any(p =>
                p.EmployeeId == attendanceRecord.EmployeeId && p.WorkDay == attendanceRecord.WorkDay))
            {
                ModelState.AddModelError(string.Empty,
                    "Unable to save this Presence Record. The entry with this work day date already exists. "
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
                || (await _userManager.IsInRoleAsync(await _userManager.FindByIdAsync(currentUserId),
                    Roles.TeamAdministratorRole.ToString())))
            {
                ViewData["ManagerApprovalControlDisabled"] = false;
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

            var attendanceRecord = await _context.AttendanceRecords.Include(p => p.Employee).AsNoTracking()
                .FirstOrDefaultAsync(p => p.EmployeeId == employeeId && p.WorkDay == workday);
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
            _logger.LogInformation("Deleteting item with empoloyeeId={employeeId}, workday={workday}", employeeId,
                workday.ToShortDateString());
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
            ViewData["MorningAttendance"] =
                new SelectList(Enum.GetNames(typeof(Attendance)), attendanceRecord.MorningAttendance);
            ViewData["AfternoonAttendance"] =
                new SelectList(Enum.GetNames(typeof(Attendance)), attendanceRecord.AfternoonAttendance);
            ViewData["ManagerApprovalStatus"] = new SelectList(Enum.GetNames(typeof(ManagerApprovalStatus)),
                attendanceRecord.ManagerApprovalStatus);
            ViewData["ManagerApprovalControlDisabled"] = true;
        }
    }
}