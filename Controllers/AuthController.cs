using GestionDeConsorciosMVC.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace GestionDeConsorciosMVC.Controllers
{
    public class AuthController : Controller
    {
        private readonly GestionDeConsorciosContext _context;

        public AuthController(GestionDeConsorciosContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            HttpContext.Session.Clear();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password, string role)
        {
            email = email?.Trim() ?? string.Empty;
            role = role?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(role))
            {
                ViewBag.Error = "Completa email, contrasena y rol para continuar.";
                return View();
            }

            if (role.Equals("Propietario", StringComparison.OrdinalIgnoreCase))
            {
                var usuario = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.Email == email && u.Rol == RolUsuario.Propietario && u.Activo);

                if (usuario is null)
                {
                    ViewBag.Error = "No existe un propietario activo con ese email.";
                    return View();
                }

                var tieneUnidadFuncional = await _context.UnidadesFuncionales
                    .AnyAsync(unidad => unidad.MailPropietario.ToLower() == email.ToLower());

                if (!tieneUnidadFuncional)
                {
                    ViewBag.Error = "El propietario no tiene unidades funcionales asociadas a ese email.";
                    return View();
                }
            }

            HttpContext.Session.SetString("UserEmail", email);
            HttpContext.Session.SetString("UserRole", role);

            return role.Equals("Propietario", StringComparison.OrdinalIgnoreCase)
                ? RedirectToAction("PropietarioDashboard", "Home")
                : RedirectToAction("AdminDashboard", "Home");
        }
    }
}
