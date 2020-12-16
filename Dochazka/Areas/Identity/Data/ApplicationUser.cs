using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Dochazka.Models;
using Microsoft.AspNetCore.Identity;

namespace Dochazka.Areas.Identity.Data
{
    public class ApplicationUser : IdentityUser
    {

        public string FirstName { get; set; }
        public string LastName { get; set; }

        [Display(Name = "Full Name")]
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
              
        public Team Team { get; set; }

    }
}
