using Biblio.Models;
using Biblio.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Biblio.Controllers
{
    [AllowAnonymous]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        // --- (صفحات Features Section) ---
        public IActionResult Catalog()
        {
            return View();
        }

        public IActionResult Features()
        {
            return View();
        }

        public IActionResult Events()
        {
            return View();
        }
        public IActionResult Contact()
        {
            return View();
        }

        // --- (صفحات Bottom Links) ---
        public IActionResult DataProtection()
        {
            return View();
        }

        public IActionResult CookiePreferences()
        {
            return View();
        }

        public IActionResult UserAgreement()
        {
            return View();
        }

    }
}
