using Dochazka.Areas.Identity.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Dochazka.Models
{
    public class Team
    {
        public int TeamID { get; set; }

        [Required]
        [Display(Name = "Team Name")]
        [StringLength(50, ErrorMessage = "First name cannot be longer than 50 characters.")]
        public string TeamName { get; set; }

        // user ID from AspNetUser table.        
        public string PrimaryManagerId { get; set; }

        // navigation property
        public ApplicationUser PrimaryManager { get; set; }
    }
}
