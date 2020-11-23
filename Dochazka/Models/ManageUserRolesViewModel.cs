using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dochazka.Models
{
    /// <summary>
    /// Model which helps to manage user roles in UserRolesController
    /// </summary>
    public class ManageUserRolesViewModel
    {
        public string RoleId { get; set; }
        public string RoleName { get; set; }
        public bool Selected { get; set; }
    }
}
