using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Dochazka.Models
{
    public class PresenceRecord
    {
        //public int PresenceRecordId { get; set; }

        [Display(Name = "WorkDay Date"), DataType(DataType.Date), DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        public DateTime WorkDay { get; set; }

        //private string _WorkDayKey;

        //// to be used as a part of composite primary key 
        //public string WorkDayKey {
        //    get => _WorkDayKey;
        //    set => _WorkDayKey = this.WorkDay.Date.ToString(CultureInfo.InvariantCulture);
        //}

        public DayTimeSlot DayTimeSlot { get; set; }

        // user ID from AspNetUser table.
        public string employeeId { get; set; }

        public PresenceState Presence { get; set; }

        public ManagerApprovalStatus ManagerApprovalStatus { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }

        // navigation property
        public IdentityUser Employee { get; set; }
    }

    public enum DayTimeSlot
    {
        Morning,
        Afternoon
    }   
}
