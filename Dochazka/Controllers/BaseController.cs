using Dochazka.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Dochazka.Areas.Identity.Data;


namespace Dochazka.Controllers
{
    public abstract class BaseController : Controller
    {
        protected readonly ApplicationDbContext _context;
        protected readonly IAuthorizationService _authorizationService;
        protected readonly UserManager<ApplicationUser> _userManager;        

        public BaseController(

            ApplicationDbContext context,
            IAuthorizationService authorizationService,
            UserManager<ApplicationUser> userManager) : base()
        {
            _context = context;            
            _authorizationService = authorizationService;
            _userManager = userManager;
        }
    }
}
