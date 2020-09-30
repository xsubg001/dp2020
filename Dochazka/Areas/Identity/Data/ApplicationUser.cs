using System;
using System.ComponentModel.DataAnnotations;
using Dochazka.Models;
using Microsoft.AspNetCore.Identity;

namespace Dochazka.Areas.Identity.Data
{
    public class ApplicationUser : IdentityUser
    {

        public string FirstName { get; set; }
        public string LastName { get; set; }
        
        public string FullName
        {
            get
            {
                if (!string.IsNullOrEmpty(FirstName) || string.IsNullOrEmpty(LastName))
                {
                    return LastName + " " + FirstName;
                }
                return string.Empty;
            }
        }

        // user ID from AspNetUser table.
        public string ManagerId { get; set; }
                
        // navigation property reference to the team
        public string TeamId { get; set; }

        // navigation property
        public ApplicationUser Manager { get; set; }

        // navigation property to the team
        public Team Team { get; set; }
    }
}
