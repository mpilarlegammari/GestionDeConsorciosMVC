using GestionDeConsorciosMVC.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace GestionDeConsorciosMVC.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return RedirectToAction("Login", "Auth");
        }

        public IActionResult AdminDashboard()
        {
            return View();
        }

        public IActionResult PropietarioDashboard()
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
    }
}
