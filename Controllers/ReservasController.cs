using GestionDeConsorciosMVC.Services;
using GestionDeConsorciosMVC.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GestionDeConsorciosMVC.Controllers
{
    public class ReservasController : Controller
    {
        private readonly IReservasService _reservasService;

        public ReservasController(IReservasService reservasService)
        {
            _reservasService = reservasService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(EstadoReserva? estado, int? consorcioId, DateTime? fecha, string? busqueda)
        {
            var model = await _reservasService.GetAdminIndexAsync(estado, consorcioId, fecha, busqueda);
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var email = HttpContext.Session.GetString("UserEmail");

            if (string.IsNullOrWhiteSpace(email))
            {
                return RedirectToAction("Login", "Auth");
            }

            var model = await _reservasService.BuildReservaCreateViewModelAsync(email);

            if (model.UnidadesFuncionales.Count == 0)
            {
                TempData["Error"] = "No tenes unidades funcionales asociadas para crear una reserva.";
            }
            else if (model.Amenities.Count == 0)
            {
                TempData["Error"] = "No hay amenities activos disponibles para tus consorcios.";
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ReservaCreateViewModel model)
        {
            var email = HttpContext.Session.GetString("UserEmail");

            if (string.IsNullOrWhiteSpace(email))
            {
                return RedirectToAction("Login", "Auth");
            }

            var errors = await _reservasService.ValidateReservaAsync(model, email);

            foreach (var error in errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }

            if (!ModelState.IsValid)
            {
                TempData["Error"] = string.Join(" ", ModelState.Values
                    .SelectMany(value => value.Errors)
                    .Select(error => error.ErrorMessage));

                var viewModel = await _reservasService.BuildReservaCreateViewModelAsync(email, model);
                return View(viewModel);
            }

            var reserva = await _reservasService.CreateReservaAsync(model);

            TempData["Success"] = "Reserva creada correctamente.";
            return RedirectToAction(nameof(Details), new { id = reserva.Id, rol = "propietario" });
        }

        [HttpGet]
        public async Task<IActionResult> MisReservas(EstadoReserva? estado, string? busqueda)
        {
            var email = HttpContext.Session.GetString("UserEmail");

            if (string.IsNullOrWhiteSpace(email))
            {
                return RedirectToAction("Login", "Auth");
            }

            var model = await _reservasService.GetMisReservasAsync(email, estado, busqueda);
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id, string? rol, string? returnUrl)
        {
            var model = await _reservasService.GetDetailsAsync(id);

            if (model is null)
            {
                return NotFound();
            }

            var role = HttpContext.Session.GetString("UserRole");
            var esPropietario = role?.Equals("Propietario", StringComparison.OrdinalIgnoreCase) == true
                || rol?.Equals("propietario", StringComparison.OrdinalIgnoreCase) == true;

            if (esPropietario)
            {
                var email = HttpContext.Session.GetString("UserEmail");

                if (string.IsNullOrWhiteSpace(email))
                {
                    return RedirectToAction("Login", "Auth");
                }

                if (!await _reservasService.OwnerCanAccessAsync(id, email))
                {
                    return Forbid();
                }
            }

            model.DetailRole = esPropietario ? "Propietario" : "Administrador";
            model.ReturnUrl = GetSafeReturnUrl(returnUrl, esPropietario);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancelar(int id, string? returnUrl)
        {
            var email = HttpContext.Session.GetString("UserEmail");

            if (string.IsNullOrWhiteSpace(email))
            {
                return RedirectToAction("Login", "Auth");
            }

            var result = await _reservasService.CancelarAsync(id, email);

            if (!result.Success)
            {
                TempData["Error"] = result.Error ?? "No se pudo cancelar la reserva.";
                return RedirectToAction(nameof(Details), new { id, rol = "propietario", returnUrl });
            }

            TempData["Success"] = "Reserva cancelada correctamente.";

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction(nameof(MisReservas));
        }

        private string GetSafeReturnUrl(string? returnUrl, bool esPropietario)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return returnUrl;
            }

            return esPropietario
                ? Url.Action(nameof(MisReservas), "Reservas") ?? "/Reservas/MisReservas"
                : Url.Action(nameof(Index), "Reservas") ?? "/Reservas";
        }
    }
}
