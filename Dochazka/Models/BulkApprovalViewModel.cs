using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Dochazka.Models
{
    public class BulkApprovalViewModel
    {
        public DateTime[] WorkDays { get; set; }

        public string[] EmployeeIds { get; set; }

        public ManagerApprovalStatus[] ManagerApprovalStatuses { get; set; }
    }
}
