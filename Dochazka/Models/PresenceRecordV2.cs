using Dochazka.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Dochazka.Models
{
    public class PresenceRecordV2
    {

        [Display(Name = "Work Day Date"), DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = false)]
        public DateTime WorkDay { get; set; }

        [Display(Name = "Morning Presence")]
        public PresenceState MorningPresence { get; set; }

        [Display(Name = "Afternoon Presence")]
        public PresenceState AfternoonPresence { get; set; }

        // user ID from AspNetUser table.
        public string EmployeeId { get; set; }
        [Display(Name = "Manager Approval Status")]
        public ManagerApprovalStatus ManagerApprovalStatus { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }

        // navigation property
        public ApplicationUser Employee { get; set; }

        public PresenceRecordV2()
        {            
            AfternoonPresence = MorningPresence = PresenceState.Absence;
            ManagerApprovalStatus = ManagerApprovalStatus.Submitted;
        }
    }           
}
