using Microsoft.AspNetCore.Mvc;

namespace GestionDeConsorciosMVC.Controllers
{
    public class AuthController : Controller
    {
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string email, string password, string role)
        {
            TempData["LoginMockMessage"] = $"Ingreso mock como {role}. No se validaron credenciales.";

            return RedirectToAction("Index", "Home");
        }
    }
}
