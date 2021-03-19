using Dochazka.Areas.Identity.Data;
using System;
using System.ComponentModel.DataAnnotations;


namespace Dochazka.Models
{
    /// <summary>
    /// Model which persists AttendanceRecords for single working day and employee. It makes use of composite key, which is made from WorkDay and EmployeeId. 
    /// See ApplicationDbContext.cs
    /// </summary>
    public class AttendanceRecordModel
    {

        [Display(Name = "Work Day Date"), DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = false)]
        public DateTime WorkDay { get; set; }

        [Display(Name = "Morning Attendance")]
        public Attendance MorningAttendance { get; set; }

        [Display(Name = "Afternoon Attendance")]
        public Attendance AfternoonAttendance { get; set; }

        // navigation property, user ID from AspNetUser table.
        public string EmployeeId { get; set; }

        // navigation property
        public ApplicationUser Employee { get; set; }


        [Display(Name = "Manager Approval Status")]
        public ManagerApprovalStatus ManagerApprovalStatus { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }


        public AttendanceRecordModel()
        {            
            AfternoonAttendance = MorningAttendance = Attendance.Absence;
            ManagerApprovalStatus = ManagerApprovalStatus.Submitted;
        }
    }           
}
