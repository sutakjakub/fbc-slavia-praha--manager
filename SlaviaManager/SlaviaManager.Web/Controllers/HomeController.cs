using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SlaviaManager.Web.Auth;
using SlaviaManager.Web.Data;
using SlaviaManager.Web.Entities;

namespace SlaviaManager.Web.Controllers
{
    [Authorize(Roles = CustomRoles.Management)]
    [Route("api/[controller]/[action]")]
    public class HomeController : Controller
    {
        private readonly ClaimsPrincipal _caller;
        private readonly ApplicationDbContext _appDbContext;

        public HomeController(UserManager<AppUserEntity> userManager, ApplicationDbContext appDbContext, IHttpContextAccessor httpContextAccessor)
        {
            _caller = httpContextAccessor.HttpContext.User;
            _appDbContext = appDbContext;
        }

        // GET api/dashboard/home
        [Authorize(Policy = CustomClaims.EditUserPermissions)]
        [HttpGet]
        public async Task<IActionResult> Home()
        {
            // retrieve the user info
            //HttpContext.User
            var userId = _caller.Claims.Single(c => c.Type == "id");

            return new OkObjectResult($"User '{userId}' has permission :)");
        }
    }
}
