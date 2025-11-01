using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Biblio.Models; 
using Biblio.Models.ViewModels;
using System.Threading.Tasks;

namespace Biblio.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;

        public AccountController(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return NotFound();

            var roles = await _userManager.GetRolesAsync(user);

            var vm = new ProfileViewModel
            {
                FullName = user.FullName,
                Email = user.Email,
                PlanType = user.PlanType.ToString(),
                Roles = roles
            };

            return View(vm);
        }
    }
}
