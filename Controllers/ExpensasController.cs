using GestionDeConsorciosMVC.Context;
using GestionDeConsorciosMVC.Services;
using GestionDeConsorciosMVC.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestionDeConsorciosMVC.Controllers
{
    public class ExpensasController : Controller
    {
        private readonly GestionDeConsorciosContext _context;
        private readonly IExpensasService _expensasService;

        public ExpensasController(GestionDeConsorciosContext context, IExpensasService expensasService)
        {
            _context = context;
            _expensasService = expensasService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            int? consorcioId,
            string? periodo,
            int? mes,
            int? anio,
            EstadoExpensa? estado)
        {
            var model = await _expensasService.GetIndexAsync(consorcioId, periodo, mes, anio, estado);
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Generar(int? consorcioId, string? periodo)
        {
            var model = new GenerarExpensasViewModel
            {
                ConsorcioId = consorcioId ?? 0,
                Periodo = string.IsNullOrWhiteSpace(periodo) ? DateTime.Today.ToString("yyyy-MM") : periodo
            };

            return View(await _expensasService.BuildGenerarViewModelAsync(model));
        }

        [HttpGet]
        public async Task<IActionResult> MisExpensas()
        {
            var email = HttpContext.Session.GetString("UserEmail");

            if (string.IsNullOrWhiteSpace(email))
            {
                return RedirectToAction("Login", "Auth");
            }

            var expensas = await _context.Expensas
                .Include(e => e.UnidadFuncional)
                    .ThenInclude(u => u.Consorcio)
                .Include(e => e.Pagos)
                .Where(e => e.UnidadFuncional.MailPropietario == email)
                .OrderByDescending(e => e.FechaEmision)
                .ToListAsync();

            return View(expensas);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id, string? returnUrl = null)
        {
            var role = HttpContext.Session.GetString("UserRole");
            var email = HttpContext.Session.GetString("UserEmail");
            var model = await _expensasService.GetDetailsAsync(id);

            if (model is null)
            {
                return NotFound();
            }

            if (role?.Equals("Propietario", StringComparison.OrdinalIgnoreCase) == true
                && !string.Equals(model.PropietarioMail, email, StringComparison.OrdinalIgnoreCase))
            {
                return Forbid();
            }

            model.ReturnUrl = GetSafeReturnUrl(returnUrl);
            model.DetailRole = model.ReturnUrl == Url.Action(nameof(MisExpensas), "Expensas")
                ? "Propietario"
                : "Administrador";

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Generar(GenerarExpensasViewModel model)
        {
            await ValidateGeneracionAsync(model);

            if (!ModelState.IsValid)
            {
                TempData["Error"] = string.Join(" ", ModelState.Values
                    .SelectMany(value => value.Errors)
                    .Select(error => error.ErrorMessage));

                return View(await _expensasService.BuildGenerarViewModelAsync(model));
            }

            var generadas = await _expensasService.GenerarAsync(model);

            TempData["Success"] = $"Se generaron {generadas} expensas para el periodo {model.Periodo}.";
            return RedirectToAction(nameof(Index));
        }

        private async Task ValidateGeneracionAsync(GenerarExpensasViewModel model)
        {
            model.Periodo = model.Periodo?.Trim() ?? string.Empty;
            model.CriterioDistribucion = model.CriterioDistribucion?.Trim() ?? string.Empty;
            model.Observaciones = string.IsNullOrWhiteSpace(model.Observaciones) ? null : model.Observaciones.Trim();

            var consorcio = await _expensasService.GetConsorcioConUnidadesAsync(model.ConsorcioId);

            if (consorcio is null)
            {
                ModelState.AddModelError(nameof(model.ConsorcioId), "Debe seleccionar un consorcio valido.");
            }
            else if (consorcio.UnidadesFuncionales.Count == 0)
            {
                ModelState.AddModelError(nameof(model.ConsorcioId), "El consorcio debe tener unidades funcionales cargadas.");
            }

            if (string.IsNullOrWhiteSpace(model.Periodo))
            {
                ModelState.AddModelError(nameof(model.Periodo), "El periodo es obligatorio.");
            }

            if (model.FechaEmision == default)
            {
                ModelState.AddModelError(nameof(model.FechaEmision), "La fecha de emision es obligatoria.");
            }

            if (model.FechaVencimiento <= model.FechaEmision)
            {
                ModelState.AddModelError(nameof(model.FechaVencimiento), "El vencimiento debe ser posterior a la emision.");
            }

            if (string.IsNullOrWhiteSpace(model.CriterioDistribucion))
            {
                ModelState.AddModelError(nameof(model.CriterioDistribucion), "Debe seleccionar un criterio de distribucion.");
            }

            if (consorcio is not null && !string.IsNullOrWhiteSpace(model.Periodo))
            {
                var yaGeneradas = await _expensasService.ExistsPeriodoGeneradoAsync(model.ConsorcioId, model.Periodo);

                if (yaGeneradas)
                {
                    ModelState.AddModelError(nameof(model.Periodo), "Ya existen expensas generadas para ese consorcio y periodo.");
                }

                var gastos = await _expensasService.GetGastosPeriodoAsync(model.ConsorcioId, model.Periodo);

                if (gastos.Count == 0)
                {
                    ModelState.AddModelError(nameof(model.Periodo), "No hay gastos cargados para ese consorcio y periodo.");
                }
            }
        }

        private string GetSafeReturnUrl(string? returnUrl)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return returnUrl;
            }

            return Url.Action(nameof(Index), "Expensas") ?? "/Expensas";
        }
    }
}
