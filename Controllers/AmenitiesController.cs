using GestionDeConsorciosMVC.Services;
using GestionDeConsorciosMVC.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GestionDeConsorciosMVC.Controllers
{
    public class AmenitiesController : Controller
    {
        private readonly IReservasService _reservasService;

        public AmenitiesController(IReservasService reservasService)
        {
            _reservasService = reservasService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? consorcioId, bool? activo, string? busqueda)
        {
            var model = await _reservasService.GetAmenitiesIndexAsync(consorcioId, activo, busqueda);
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = await _reservasService.BuildAmenityCreateViewModelAsync();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AmenityCreateViewModel model)
        {
            if (!await _reservasService.ConsorcioExistsAsync(model.ConsorcioId))
            {
                ModelState.AddModelError(nameof(model.ConsorcioId), "Debe seleccionar un consorcio valido.");
            }

            if (!ModelState.IsValid)
            {
                TempData["Error"] = string.Join(" ", ModelState.Values
                    .SelectMany(value => value.Errors)
                    .Select(error => error.ErrorMessage));

                var viewModel = await _reservasService.BuildAmenityCreateViewModelAsync(model);
                return View(viewModel);
            }

            await _reservasService.CreateAmenityAsync(model);

            TempData["Success"] = "Amenity creado correctamente.";
            return RedirectToAction(nameof(Index));
        }
    }
}
