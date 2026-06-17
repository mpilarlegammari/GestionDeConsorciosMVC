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
        public async Task<IActionResult> Login(string email, string password)
        {
            email = email?.Trim() ?? string.Empty;
            password = password?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Completa email y contrasena para continuar.";
                return View();
            }

            // Entrega academica: se compara texto plano. En produccion debe usarse hash real y verificacion segura.
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u =>
                    u.Email == email &&
                    u.PasswordHash == password &&
                    u.Activo);

            if (usuario is null)
            {
                ViewBag.Error = "Email o contrasena invalidos, o el usuario esta inactivo.";
                return View();
            }

            if (usuario.Rol == RolUsuario.Propietario)
            {
                var tieneUnidadFuncional = await _context.UnidadesFuncionales
                    .AnyAsync(unidad =>
                        unidad.MailPropietario != null &&
                        unidad.MailPropietario.ToLower() == usuario.Email.ToLower());

                if (!tieneUnidadFuncional)
                {
                    ViewBag.Error = "El propietario no tiene una unidad funcional asociada.";
                    return View();
                }
            }

            HttpContext.Session.SetString("UserEmail", usuario.Email);
            HttpContext.Session.SetString("UserRole", usuario.Rol.ToString());

            return usuario.Rol == RolUsuario.Propietario
                ? RedirectToAction("PropietarioDashboard", "Home")
                : RedirectToAction("AdminDashboard", "Home");
        }
    }
}
