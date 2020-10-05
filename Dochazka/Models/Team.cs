using Dochazka.Areas.Identity.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Dochazka.Models
{
    public class Team
    {
        public int TeamId { get; set; }

        [Required]
        [Display(Name = "Team Name")]
        [StringLength(50, ErrorMessage = "First name cannot be longer than 50 characters.")]
        public string TeamName { get; set; }

        // navigation property, user ID from AspNetUser table.        
        public string PrimaryManagerId { get; set; }        
        public ApplicationUser PrimaryManager { get; set; }

        [InverseProperty("Team")]
        public List<ApplicationUser> TeamMembers { get; set; }
    }
}
