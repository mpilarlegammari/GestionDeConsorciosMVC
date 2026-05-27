using GestionDeConsorciosMVC.Services;
using GestionDeConsorciosMVC.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GestionDeConsorciosMVC.Controllers
{
    public class ConsorciosController : Controller
    {
        private readonly IConsorcioService _consorcioService;

        public ConsorciosController(IConsorcioService consorcioService)
        {
            _consorcioService = consorcioService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var consorcios = await _consorcioService.GetAllAsync();
            return View(consorcios);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var consorcio = await _consorcioService.GetByIdAsync(id);

            if (consorcio is null)
            {
                return NotFound();
            }

            return View(consorcio);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(CreateEmptyViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ConsorcioVM model)
        {
            ValidateUnidadesFuncionales(model);

            if (!ModelState.IsValid)
            {
                EnsureAtLeastOneUnidad(model);
                return View(model);
            }

            var consorcio = await _consorcioService.CreateAsync(model);
            return RedirectToAction(nameof(Details), new { id = consorcio.Id });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var consorcio = await _consorcioService.GetForEditAsync(id);

            if (consorcio is null)
            {
                return NotFound();
            }

            return View(consorcio);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ConsorcioVM model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var updated = await _consorcioService.UpdateGeneralAsync(model);

            if (!updated)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Details), new { id = model.Id });
        }

        private void ValidateUnidadesFuncionales(ConsorcioVM model)
        {
            if (model.UnidadesFuncionales.Count == 0)
            {
                ModelState.AddModelError(string.Empty, "Debe cargar al menos una unidad funcional.");
                return;
            }

            var duplicateNumbers = model.UnidadesFuncionales
                .Where(unidad => unidad.NumeroUF > 0)
                .GroupBy(unidad => unidad.NumeroUF)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key)
                .ToHashSet();

            if (duplicateNumbers.Count == 0)
            {
                return;
            }

            ModelState.AddModelError(string.Empty, "No pueden existir dos unidades funcionales con el mismo numero de UF.");

            for (var index = 0; index < model.UnidadesFuncionales.Count; index++)
            {
                if (duplicateNumbers.Contains(model.UnidadesFuncionales[index].NumeroUF))
                {
                    ModelState.AddModelError(
                        $"UnidadesFuncionales[{index}].NumeroUF",
                        "Numero de UF duplicado.");
                }
            }
        }

        private static ConsorcioVM CreateEmptyViewModel()
        {
            return new ConsorcioVM
            {
                UnidadesFuncionales = new List<UnidadFuncionalVM>
                {
                    new()
                }
            };
        }

        private static void EnsureAtLeastOneUnidad(ConsorcioVM model)
        {
            if (model.UnidadesFuncionales.Count == 0)
            {
                model.UnidadesFuncionales.Add(new UnidadFuncionalVM());
            }
        }
    }
}