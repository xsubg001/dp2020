using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dochazka.Data
{
    public enum Roles
    {        
        TeamAdministratorRole,
        TeamManagerRole,
        TeamMemberRole,        
        ContactManagersRole, // legacy, to be deprecated
        ContactAdministratorsRole // legacy, to be deprecated
    }
}
