using GestionDeConsorciosMVC.Services;
using GestionDeConsorciosMVC.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GestionDeConsorciosMVC.Controllers
{
    public class ComunicadosController : Controller
    {
        private const long MaxAdjuntoBytes = 5 * 1024 * 1024;
        private static readonly string[] AllowedExtensions = [".pdf", ".jpg", ".jpeg", ".png"];
        private readonly IComunicadosService _comunicadosService;
        private readonly IFileStorageService _fileStorageService;

        public ComunicadosController(IComunicadosService comunicadosService, IFileStorageService fileStorageService)
        {
            _comunicadosService = comunicadosService;
            _fileStorageService = fileStorageService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? consorcioId, bool? importante, string? busqueda)
        {
            if (IsPropietario())
            {
                return RedirectToAction(nameof(MisComunicados));
            }

            var model = await _comunicadosService.GetAdminIndexAsync(consorcioId, importante, busqueda);
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            if (IsPropietario())
            {
                return RedirectToAction(nameof(MisComunicados));
            }

            var model = await _comunicadosService.BuildCreateViewModelAsync();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ComunicadoCreateViewModel model, IFormFile? archivoAdjunto)
        {
            if (IsPropietario())
            {
                return RedirectToAction(nameof(MisComunicados));
            }

            if (!await _comunicadosService.ConsorcioExistsAsync(model.ConsorcioId))
            {
                ModelState.AddModelError(nameof(model.ConsorcioId), "Debe seleccionar un consorcio valido.");
            }

            ValidateAdjunto(archivoAdjunto);

            if (!ModelState.IsValid)
            {
                TempData["Error"] = string.Join(" ", ModelState.Values
                    .SelectMany(value => value.Errors)
                    .Select(error => error.ErrorMessage));

                var viewModel = await _comunicadosService.BuildCreateViewModelAsync(model);
                return View(viewModel);
            }

            var archivoAdjuntoPath = archivoAdjunto is null || archivoAdjunto.Length == 0
                ? null
                : await _fileStorageService.SaveAsync(archivoAdjunto, "comunicados");

            var comunicado = await _comunicadosService.CreateAsync(model, archivoAdjuntoPath);

            TempData["Success"] = "Comunicado publicado correctamente.";
            return RedirectToAction(nameof(Details), new { id = comunicado.Id });
        }

        [HttpGet]
        public async Task<IActionResult> MisComunicados(bool? importante, string? busqueda)
        {
            var email = HttpContext.Session.GetString("UserEmail");

            if (!IsPropietario() || string.IsNullOrWhiteSpace(email))
            {
                return RedirectToAction("Login", "Auth");
            }

            var model = await _comunicadosService.GetMisComunicadosAsync(email, importante, busqueda);
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id, string? rol, string? returnUrl)
        {
            var model = await _comunicadosService.GetDetailsAsync(id);

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

                if (!await _comunicadosService.OwnerCanAccessAsync(id, email))
                {
                    return Forbid();
                }
            }

            model.DetailRole = esPropietario ? "Propietario" : "Administrador";
            model.ReturnUrl = GetSafeReturnUrl(returnUrl, esPropietario);

            return View(model);
        }

        private void ValidateAdjunto(IFormFile? archivoAdjunto)
        {
            if (archivoAdjunto is null || archivoAdjunto.Length == 0)
            {
                return;
            }

            var extension = Path.GetExtension(archivoAdjunto.FileName).ToLowerInvariant();

            if (!AllowedExtensions.Contains(extension))
            {
                ModelState.AddModelError("ArchivoAdjunto", "Formato permitido: pdf, jpg, jpeg o png.");
            }

            if (archivoAdjunto.Length > MaxAdjuntoBytes)
            {
                ModelState.AddModelError("ArchivoAdjunto", "El adjunto no puede superar los 5 MB.");
            }
        }

        private string GetSafeReturnUrl(string? returnUrl, bool esPropietario)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return returnUrl;
            }

            return esPropietario
                ? Url.Action(nameof(MisComunicados), "Comunicados") ?? "/Comunicados/MisComunicados"
                : Url.Action(nameof(Index), "Comunicados") ?? "/Comunicados";
        }

        private bool IsPropietario()
        {
            return HttpContext.Session.GetString("UserRole")
                ?.Equals("Propietario", StringComparison.OrdinalIgnoreCase) == true;
        }
    }
}
