using Dochazka.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Dochazka.Controllers
{
    public abstract class DI_BaseController : Controller
    {
        protected readonly ApplicationDbContext Context;
        protected readonly IAuthorizationService AuthorizationService;
        protected readonly UserManager<IdentityUser> UserManager;        

        public DI_BaseController(

            ApplicationDbContext context,
            IAuthorizationService authorizationService,
            UserManager<IdentityUser> userManager) : base()
        {
            Context = context;
            UserManager = userManager;
            AuthorizationService = authorizationService;
        }
    }
}
