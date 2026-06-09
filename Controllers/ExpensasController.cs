using GestionDeConsorciosMVC.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace GestionDeConsorciosMVC.Controllers
{
    public class ExpensasController : Controller
    {
        private readonly GestionDeConsorciosContext _context;

        public ExpensasController(GestionDeConsorciosContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var expensas = await _context.Expensas
                .Include(e => e.UnidadFuncional)
                    .ThenInclude(u => u.Consorcio)
                .Include(e => e.Pagos)
                .OrderByDescending(e => e.FechaEmision)
                .ThenBy(e => e.UnidadFuncional.NumeroUF)
                .ToListAsync();

            ViewBag.Consorcios = await _context.Consorcios.OrderBy(c => c.Nombre).ToListAsync();
            return View(expensas);
        }

        [HttpGet]
        public async Task<IActionResult> Generar()
        {
            await LoadGenerarDataAsync();
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> MisExpensas()
        {
            var expensas = await _context.Expensas
                .Include(e => e.UnidadFuncional)
                    .ThenInclude(u => u.Consorcio)
                .Include(e => e.Pagos)
                .OrderByDescending(e => e.FechaEmision)
                .ToListAsync();

            return View(expensas);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id, string? returnUrl = null)
        {
            var expensa = await _context.Expensas
                .Include(e => e.UnidadFuncional)
                    .ThenInclude(u => u.Consorcio)
                .Include(e => e.Pagos)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (expensa is null)
            {
                return NotFound();
            }

            ViewBag.Gastos = await GetGastosPeriodoAsync(expensa.UnidadFuncional.ConsorcioId, expensa.Periodo);
            ViewBag.ReturnUrl = GetSafeReturnUrl(returnUrl);
            ViewBag.DetailRole = ViewBag.ReturnUrl == Url.Action(nameof(MisExpensas), "Expensas")
                ? "Propietario"
                : "Administrador";
            return View(expensa);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Generar(
            int ConsorcioId,
            string Periodo,
            DateTime FechaEmision,
            DateTime FechaVencimiento,
            string CriterioDistribucion,
            string? Observaciones)
        {
            Periodo = Periodo?.Trim() ?? string.Empty;
            CriterioDistribucion = CriterioDistribucion?.Trim() ?? string.Empty;
            Observaciones = Observaciones?.Trim();

            var consorcio = await _context.Consorcios
                .Include(c => c.UnidadesFuncionales)
                .FirstOrDefaultAsync(c => c.Id == ConsorcioId);

            if (consorcio is null)
            {
                ModelState.AddModelError(nameof(ConsorcioId), "Debe seleccionar un consorcio valido.");
            }
            else if (consorcio.UnidadesFuncionales.Count == 0)
            {
                ModelState.AddModelError(nameof(ConsorcioId), "El consorcio debe tener unidades funcionales cargadas.");
            }

            if (string.IsNullOrWhiteSpace(Periodo))
            {
                ModelState.AddModelError(nameof(Periodo), "El periodo es obligatorio.");
            }

            if (FechaEmision == default)
            {
                ModelState.AddModelError(nameof(FechaEmision), "La fecha de emision es obligatoria.");
            }

            if (FechaVencimiento <= FechaEmision)
            {
                ModelState.AddModelError(nameof(FechaVencimiento), "El vencimiento debe ser posterior a la emision.");
            }

            if (string.IsNullOrWhiteSpace(CriterioDistribucion))
            {
                ModelState.AddModelError(nameof(CriterioDistribucion), "Debe seleccionar un criterio de distribucion.");
            }

            var yaGeneradas = consorcio is not null && await _context.Expensas
                .AnyAsync(e => e.Periodo == Periodo && e.UnidadFuncional.ConsorcioId == ConsorcioId);

            if (yaGeneradas)
            {
                ModelState.AddModelError(nameof(Periodo), "Ya existen expensas generadas para ese consorcio y periodo.");
            }

            var gastos = consorcio is null
                ? []
                : await GetGastosPeriodoAsync(ConsorcioId, Periodo);

            if (gastos.Count == 0)
            {
                ModelState.AddModelError(nameof(Periodo), "No hay gastos cargados para ese consorcio y periodo.");
            }

            if (!ModelState.IsValid || consorcio is null)
            {
                TempData["Error"] = string.Join(" ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                await LoadGenerarDataAsync(ConsorcioId, Periodo);
                return View();
            }

            var total = gastos.Sum(g => g.Monto);
            var montoPorUnidad = Math.Round(total / consorcio.UnidadesFuncionales.Count, 2);

            foreach (var unidad in consorcio.UnidadesFuncionales)
            {
                _context.Expensas.Add(new Expensa
                {
                    UnidadFuncionalId = unidad.Id,
                    Periodo = Periodo,
                    FechaEmision = FechaEmision,
                    FechaVencimiento = FechaVencimiento,
                    MontoTotal = montoPorUnidad,
                    Estado = EstadoExpensa.Pendiente,
                    Observaciones = string.IsNullOrWhiteSpace(Observaciones) ? null : Observaciones
                });
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Se generaron {consorcio.UnidadesFuncionales.Count} expensas para {consorcio.Nombre}.";
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadGenerarDataAsync(int? consorcioId = null, string? periodo = null)
        {
            ViewBag.Consorcios = await _context.Consorcios
                .Include(c => c.UnidadesFuncionales)
                .OrderBy(c => c.Nombre)
                .ToListAsync();

            ViewBag.Gastos = consorcioId is null || string.IsNullOrWhiteSpace(periodo)
                ? new List<Gasto>()
                : await GetGastosPeriodoAsync(consorcioId.Value, periodo);
        }

        private async Task<List<Gasto>> GetGastosPeriodoAsync(int consorcioId, string periodo)
        {
            if (!DateTime.TryParse($"{periodo}-01", out var inicio))
            {
                return [];
            }

            var fin = inicio.AddMonths(1);
            return await _context.Gastos
                .Where(g => g.ConsorcioId == consorcioId && g.Fecha >= inicio && g.Fecha < fin)
                .OrderBy(g => g.Fecha)
                .ToListAsync();
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
