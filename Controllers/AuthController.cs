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
            password = password?.Trim() ?? string.Empty;
            role = role?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(role))
            {
                ViewBag.Error = "Completa email, contrasena y rol para continuar.";
                return View();
            }

            if (!Enum.TryParse<RolUsuario>(role, ignoreCase: true, out var rolUsuario))
            {
                ViewBag.Error = "Debe seleccionar un rol valido.";
                return View();
            }

            // Entrega academica: se compara texto plano. En produccion debe usarse hash real y verificacion segura.
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u =>
                    u.Email == email &&
                    u.PasswordHash == password &&
                    u.Rol == rolUsuario &&
                    u.Activo);

            if (usuario is null)
            {
                ViewBag.Error = "Email, contrasena o rol invalidos, o el usuario esta inactivo.";
                return View();
            }

            if (rolUsuario == RolUsuario.Propietario)
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
            HttpContext.Session.SetString("UserRole", rolUsuario.ToString());

            return rolUsuario == RolUsuario.Propietario
                ? RedirectToAction("PropietarioDashboard", "Home")
                : RedirectToAction("AdminDashboard", "Home");
        }
    }
}
