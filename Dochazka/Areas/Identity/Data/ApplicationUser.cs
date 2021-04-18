using System.ComponentModel.DataAnnotations;
using Dochazka.Models;
using Microsoft.AspNetCore.Identity;

namespace Dochazka.Areas.Identity.Data
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [DataType(DataType.Text)]
        [StringLength(50, ErrorMessage = "First name cannot be longer than 50 characters.")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [DataType(DataType.Text)]
        [StringLength(50, ErrorMessage = "First name cannot be longer than 50 characters.")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Display(Name = "Full Name")]
        public string FullName
        {
            get
            {
                if (!string.IsNullOrEmpty(FirstName) || !string.IsNullOrEmpty(LastName))
                {
                    return LastName + " " + FirstName;
                }
                return string.Empty;
            }
        }
                
        public TeamModel Team { get; set; }
    }
}
