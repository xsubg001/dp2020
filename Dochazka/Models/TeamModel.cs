using Dochazka.Areas.Identity.Data;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dochazka.Models
{
    /// <summary>
    /// This is a model, which stores information about Teams in the DB context, i.e. Team name, Team membership and Team Manager
    /// </summary>
    public class TeamModel
    {       

        //This is a primary key for the model
        public int TeamModelId { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Please specify a team name"), Display(Name = "Team Name"),
            StringLength(50, ErrorMessage = "Team name cannot be longer than 50 characters.")]
        public string TeamName { get; set; }

        // navigation property, user ID from AspNetUser table.
        [Display(Name = "Team Manager"), Required(AllowEmptyStrings = false, ErrorMessage = "Please assign a manager of the team")]
        public string PrimaryManagerId { get; set; }

        [Display(Name = "Team Manager")]
        public ApplicationUser PrimaryManager { get; set; }

        [InverseProperty("Team")]
        public List<ApplicationUser> TeamMembers { get; set; }
    }
}
