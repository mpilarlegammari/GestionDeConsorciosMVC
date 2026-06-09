using GestionDeConsorciosMVC.Services;
using GestionDeConsorciosMVC.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GestionDeConsorciosMVC.Controllers
{
    public class GastosController : Controller
    {
        private const long MaxFacturaBytes = 5 * 1024 * 1024;
        private static readonly string[] AllowedFacturaExtensions = [".pdf", ".jpg", ".jpeg", ".png"];

        private readonly IGastoService _gastoService;
        private readonly IFileStorageService _fileStorageService;

        public GastosController(IGastoService gastoService, IFileStorageService fileStorageService)
        {
            _gastoService = gastoService;
            _fileStorageService = fileStorageService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? consorcioId)
        {
            var gastos = await _gastoService.GetAllAsync(consorcioId);
            ViewBag.Consorcios = await _gastoService.GetConsorciosAsync();
            ViewData["ConsorcioId"] = consorcioId;

            return View(gastos);
        }

        [HttpGet]
        public async Task<IActionResult> MisGastos()
        {
            var gastos = await _gastoService.GetAllAsync();

            return View(gastos);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var gasto = await _gastoService.GetByIdAsync(id);

            if (gasto is null)
            {
                return NotFound();
            }

            return View(gasto);
        }

        [HttpGet]
        public async Task<IActionResult> Create(int? consorcioId)
        {
            var model = new GastoVM
            {
                ConsorcioId = consorcioId ?? 0,
                Fecha = DateTime.Today
            };

            await PopulateSelectionsAsync(model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GastoVM model, IFormFile? ArchivoFactura)
        {
            await ValidateFacturaAsync(model, ArchivoFactura, required: true);

            if (!ModelState.IsValid)
            {
                TempData["Error"] = string.Join(" ", ModelState.Values
                    .SelectMany(value => value.Errors)
                    .Select(error => error.ErrorMessage));
                await PopulateSelectionsAsync(model);
                return View(model);
            }

            var facturaPath = await _fileStorageService.SaveAsync(ArchivoFactura!, "gastos");
            var gasto = await _gastoService.CreateAsync(model, facturaPath);

            TempData["Success"] = "Gasto registrado correctamente.";
            return RedirectToAction(nameof(Details), new { id = gasto.Id });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var model = await _gastoService.GetForEditAsync(id);

            if (model is null)
            {
                return NotFound();
            }

            await PopulateSelectionsAsync(model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, GastoVM model, IFormFile? ArchivoFactura)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            await ValidateFacturaAsync(model, ArchivoFactura, required: false);

            if (!ModelState.IsValid)
            {
                TempData["Error"] = string.Join(" ", ModelState.Values
                    .SelectMany(value => value.Errors)
                    .Select(error => error.ErrorMessage));
                await PopulateSelectionsAsync(model);
                return View(model);
            }

            var facturaPath = ArchivoFactura is { Length: > 0 }
                ? await _fileStorageService.SaveAsync(ArchivoFactura, "gastos")
                : null;

            var updated = await _gastoService.UpdateAsync(model, facturaPath);

            if (!updated)
            {
                return NotFound();
            }

            TempData["Success"] = "Gasto actualizado correctamente.";
            return RedirectToAction(nameof(Details), new { id = model.Id });
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var gasto = await _gastoService.GetByIdAsync(id);

            if (gasto is null)
            {
                return NotFound();
            }

            return View(gasto);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var deleted = await _gastoService.DeleteAsync(id);

            if (!deleted)
            {
                return NotFound();
            }

            TempData["Success"] = "Gasto eliminado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        private async Task ValidateFacturaAsync(GastoVM model, IFormFile? factura, bool required)
        {
            if (await _gastoService.ExistsNumeroFacturaAsync(model.NumeroFactura, model.Id == 0 ? null : model.Id))
            {
                ModelState.AddModelError(nameof(GastoVM.NumeroFactura), "Ya existe un gasto con ese numero de factura.");
            }

            if (factura is null || factura.Length == 0)
            {
                if (required)
                {
                    ModelState.AddModelError(nameof(factura), "Debe adjuntar la factura del gasto.");
                }

                return;
            }

            if (factura.Length > MaxFacturaBytes)
            {
                ModelState.AddModelError(nameof(factura), "La factura no puede superar los 5 MB.");
            }

            var extension = Path.GetExtension(factura.FileName).ToLowerInvariant();

            if (!AllowedFacturaExtensions.Contains(extension))
            {
                ModelState.AddModelError(nameof(factura), "La factura debe ser PDF, JPG o PNG.");
            }
        }

        private async Task PopulateSelectionsAsync(GastoVM model)
        {
            ViewBag.Consorcios = await _gastoService.GetConsorciosAsync();
            ViewBag.Categorias = GetCategorias();
        }

        private static IEnumerable<string> GetCategorias()
        {
            return
            [
                "Limpieza",
                "Mantenimiento",
                "Servicios",
                "Seguridad",
                "Administracion",
                "Otros"
            ];
        }
    }
}
