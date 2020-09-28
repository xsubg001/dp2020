using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dochazka.Models
{
    public enum PresenceState
    {
        WorkingTime,
        PaidVacation,
        UnpaidVacation,
        DoctorSickness,
        Sickleave,
        Absence,
        LegalJustification
    }
}
