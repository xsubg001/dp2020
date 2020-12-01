using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Dochazka.Models
{
    /// <summary>
    /// Model which helps to manage user roles in UserRolesController
    /// </summary>
    public class ManageUserViewModel
    {
        public IList<RoleSelection> RoleSelections { get; set; } = new List<RoleSelection>();
        public string UserId { get; set; }        

        public int? TeamId { get; set; }
    }
}
