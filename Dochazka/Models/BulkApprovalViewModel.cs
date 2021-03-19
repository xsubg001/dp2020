using System;

namespace Dochazka.Models
{
    public class BulkApprovalViewModel
    {
        public DateTime[] WorkDays { get; set; }

        public string[] EmployeeIds { get; set; }

        public ManagerApprovalStatus[] ManagerApprovalStatuses { get; set; }
    }
}
