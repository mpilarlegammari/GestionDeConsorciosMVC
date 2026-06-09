using GestionDeConsorciosMVC.Services;
using GestionDeConsorciosMVC.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GestionDeConsorciosMVC.Controllers
{
    public class ReclamosController : Controller
    {
        private readonly IReclamosService _reclamosService;

        public ReclamosController(IReclamosService reclamosService)
        {
            _reclamosService = reclamosService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(EstadoReclamo? estado, int? consorcioId, string? busqueda)
        {
            var model = await _reclamosService.GetAdminIndexAsync(estado, consorcioId, busqueda);
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

            var model = await _reclamosService.BuildCreateViewModelAsync(email);

            if (model.UnidadesFuncionales.Count == 0)
            {
                TempData["Error"] = "No tenes unidades funcionales asociadas para crear un reclamo.";
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ReclamoCreateViewModel model)
        {
            var email = HttpContext.Session.GetString("UserEmail");

            if (string.IsNullOrWhiteSpace(email))
            {
                return RedirectToAction("Login", "Auth");
            }

            if (!await _reclamosService.OwnerHasUnidadesAsync(email))
            {
                ModelState.AddModelError(nameof(model.UnidadFuncionalId), "No tenes unidades funcionales asociadas para crear un reclamo.");
            }
            else if (!await _reclamosService.UnidadFuncionalBelongsToOwnerAsync(model.UnidadFuncionalId, email))
            {
                ModelState.AddModelError(nameof(model.UnidadFuncionalId), "La unidad funcional seleccionada no pertenece al propietario autenticado.");
            }

            if (!ModelState.IsValid)
            {
                TempData["Error"] = string.Join(" ", ModelState.Values
                    .SelectMany(value => value.Errors)
                    .Select(error => error.ErrorMessage));

                var viewModel = await _reclamosService.BuildCreateViewModelAsync(email, model);
                return View(viewModel);
            }

            var reclamo = await _reclamosService.CreateAsync(model);

            TempData["Success"] = "Reclamo creado correctamente.";
            return RedirectToAction(nameof(Details), new { id = reclamo.Id, rol = "propietario" });
        }

        [HttpGet]
        public async Task<IActionResult> MisReclamos(EstadoReclamo? estado, string? busqueda)
        {
            var email = HttpContext.Session.GetString("UserEmail");

            if (string.IsNullOrWhiteSpace(email))
            {
                return RedirectToAction("Login", "Auth");
            }

            var model = await _reclamosService.GetMisReclamosAsync(email, estado, busqueda);
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id, string? rol, string? returnUrl)
        {
            var model = await _reclamosService.GetDetailsAsync(id);

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

                if (!await _reclamosService.OwnerCanAccessAsync(id, email))
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
        public async Task<IActionResult> CambiarEstado(CambiarEstadoReclamoViewModel model)
        {
            if (!Enum.IsDefined(typeof(EstadoReclamo), model.Estado))
            {
                ModelState.AddModelError(nameof(model.Estado), "Debe seleccionar un estado valido.");
            }

            if (!ModelState.IsValid)
            {
                TempData["Error"] = string.Join(" ", ModelState.Values
                    .SelectMany(value => value.Errors)
                    .Select(error => error.ErrorMessage));
                return RedirectToAction(nameof(Details), new { id = model.ReclamoId });
            }

            var updated = await _reclamosService.CambiarEstadoAsync(model);

            if (!updated)
            {
                return NotFound();
            }

            TempData["Success"] = model.Estado == EstadoReclamo.Cerrado
                ? "Reclamo cerrado correctamente."
                : "Estado del reclamo actualizado correctamente.";

            return RedirectToLocalOrDetails(model);
        }

        private IActionResult RedirectToLocalOrDetails(CambiarEstadoReclamoViewModel model)
        {
            if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }

            return RedirectToAction(nameof(Details), new { id = model.ReclamoId });
        }

        private string GetSafeReturnUrl(string? returnUrl, bool esPropietario)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return returnUrl;
            }

            return esPropietario
                ? Url.Action(nameof(MisReclamos), "Reclamos") ?? "/Reclamos/MisReclamos"
                : Url.Action(nameof(Index), "Reclamos") ?? "/Reclamos";
        }
    }
}
