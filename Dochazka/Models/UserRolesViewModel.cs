using System.Collections.Generic;

namespace Dochazka.Models
{
    /// <summary>
    /// Helps to generate Index view in UserRolesController
    /// </summary>
    public class UserRolesViewModel
    {
        public string Id { get; set; }
        public string FullName { get; set; }        
        public string UserName { get; set; }
        public string Email { get; set; }
        public string TeamName { get; set; }
        public string ConcurrencyStamp { get; set; }
        public IEnumerable<string> Roles { get; set; }
        public bool CanBeDeleted { get; set; }

    }
}
