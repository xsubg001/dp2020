using System.Collections.Generic;

namespace Dochazka.Models
{
    /// <summary>
    /// Model which helps to manage user roles in UserRolesController
    /// </summary>
    public class ManageUserViewModel
    {
        public IList<RoleSelection> RoleSelections { get; set; } = new List<RoleSelection>();
        public string UserId { get; set; }        

        public int? TeamModelId { get; set; }
    }
}
