using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Dochazka.Models
{
    public class PayrollSummaryModel
    {
        public string EmployeeID { get; set; }
        public string UserName { get; set; }

        [DataType(DataType.Date)]
        public DateTime Month { get; set; }
        public int WorkingTime { get; set; }
        public int PaidVacation { get; set; }
        public int UnpaidVacation { get; set; }
        public int DoctorSickness { get; set; }
        public int Sickleave { get; set; }
        public int Absence { get; set; }
        public int LegalJustification { get; set; }
    }
}
