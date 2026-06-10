using GestionDeConsorciosMVC.Context;
using GestionDeConsorciosMVC.Services;
using GestionDeConsorciosMVC.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace GestionDeConsorciosMVC.Controllers
{
    public class PagosController : Controller
    {
        private static readonly string[] AllowedExtensions = [".pdf", ".jpg", ".jpeg", ".png"];
        private readonly GestionDeConsorciosContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly IPagosService _pagosService;

        public PagosController(GestionDeConsorciosContext context, IWebHostEnvironment environment, IPagosService pagosService)
        {
            _context = context;
            _environment = environment;
            _pagosService = pagosService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(EstadoPago? estado, int? consorcioId, string? periodo)
        {
            var model = await _pagosService.GetAdminIndexAsync(estado, consorcioId, periodo);
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> InformarPago()
        {
            if (string.IsNullOrWhiteSpace(HttpContext.Session.GetString("UserEmail")))
            {
                return RedirectToAction("Login", "Auth");
            }

            await LoadExpensasPendientesAsync();
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> MisPagos(
            int? unidadFuncionalId,
            string? periodo,
            int? anio,
            EstadoPago? estado,
            string? medioPago)
        {
            var email = HttpContext.Session.GetString("UserEmail");

            if (string.IsNullOrWhiteSpace(email))
            {
                return RedirectToAction("Login", "Auth");
            }

            var model = await _pagosService.GetMisPagosAsync(email, unidadFuncionalId, periodo, anio, estado, medioPago);

            if (model.UnidadesFuncionales.Count == 0)
            {
                TempData["Error"] = "No tenes unidades funcionales asociadas para consultar pagos.";
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id, string? returnUrl = null)
        {
            var role = HttpContext.Session.GetString("UserRole");
            var email = HttpContext.Session.GetString("UserEmail");
            var model = await _pagosService.GetDetailsAsync(id);

            if (model is null)
            {
                return NotFound();
            }

            if (role?.Equals("Propietario", StringComparison.OrdinalIgnoreCase) == true
                && !string.Equals(model.PropietarioMail, email, StringComparison.OrdinalIgnoreCase))
            {
                return Forbid();
            }

            model.DetailRole = role?.Equals("Propietario", StringComparison.OrdinalIgnoreCase) == true
                ? "Propietario"
                : "Administrador";
            model.ReturnUrl = GetSafeReturnUrl(returnUrl, model.DetailRole == "Propietario");

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Aprobar(RevisarPagoViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = string.Join(" ", ModelState.Values
                    .SelectMany(value => value.Errors)
                    .Select(error => error.ErrorMessage));
                return RedirectToAction(nameof(Details), new { id = model.PagoId });
            }

            var updated = await _pagosService.AprobarAsync(model);

            if (!updated)
            {
                return NotFound();
            }

            TempData["Success"] = "Pago aprobado correctamente. La expensa asociada quedo marcada como pagada.";
            return RedirectToLocalOrDetails(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Rechazar(RevisarPagoViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = string.Join(" ", ModelState.Values
                    .SelectMany(value => value.Errors)
                    .Select(error => error.ErrorMessage));
                return RedirectToAction(nameof(Details), new { id = model.PagoId });
            }

            var updated = await _pagosService.RechazarAsync(model);

            if (!updated)
            {
                return NotFound();
            }

            TempData["Success"] = "Pago rechazado correctamente.";
            return RedirectToLocalOrDetails(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InformarPago(
            int ExpensaId,
            DateTime FechaPago,
            decimal MontoPagado,
            string MedioPago,
            string? NumeroOperacion,
            string? BancoEntidad,
            IFormFile? ComprobantePago,
            string? Comentarios)
        {
            var email = HttpContext.Session.GetString("UserEmail");

            if (string.IsNullOrWhiteSpace(email))
            {
                return RedirectToAction("Login", "Auth");
            }

            MedioPago = MedioPago?.Trim() ?? string.Empty;
            NumeroOperacion = NumeroOperacion?.Trim();
            BancoEntidad = BancoEntidad?.Trim();
            Comentarios = Comentarios?.Trim();

            var expensa = await _context.Expensas
                .Include(e => e.UnidadFuncional)
                .FirstOrDefaultAsync(e => e.Id == ExpensaId);

            if (expensa is null)
            {
                ModelState.AddModelError(nameof(ExpensaId), "Debe seleccionar una expensa valida.");
            }
            else if (!string.Equals(expensa.UnidadFuncional.MailPropietario, email, StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(nameof(ExpensaId), "La expensa seleccionada no pertenece al propietario autenticado.");
            }

            if (FechaPago == default)
            {
                ModelState.AddModelError(nameof(FechaPago), "La fecha de pago es obligatoria.");
            }

            if (MontoPagado <= 0)
            {
                ModelState.AddModelError(nameof(MontoPagado), "El monto pagado debe ser mayor a 0.");
            }

            if (string.IsNullOrWhiteSpace(MedioPago))
            {
                ModelState.AddModelError(nameof(MedioPago), "Debe seleccionar un medio de pago.");
            }

            if (ComprobantePago is null || ComprobantePago.Length == 0)
            {
                ModelState.AddModelError(nameof(ComprobantePago), "El comprobante es obligatorio.");
            }
            else
            {
                var extension = Path.GetExtension(ComprobantePago.FileName).ToLowerInvariant();

                if (!AllowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError(nameof(ComprobantePago), "Formato permitido: pdf, jpg, jpeg o png.");
                }
            }

            if (!ModelState.IsValid)
            {
                TempData["Error"] = string.Join(" ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                await LoadExpensasPendientesAsync();
                return View();
            }

            var comprobantePath = await SaveComprobanteAsync(ComprobantePago);
            var pago = new Pago
            {
                ExpensaId = ExpensaId,
                FechaPago = FechaPago,
                MontoPagado = MontoPagado,
                MedioPago = MedioPago,
                NumeroOperacion = string.IsNullOrWhiteSpace(NumeroOperacion) ? null : NumeroOperacion,
                BancoEntidad = string.IsNullOrWhiteSpace(BancoEntidad) ? null : BancoEntidad,
                ComprobantePath = comprobantePath,
                Comentarios = string.IsNullOrWhiteSpace(Comentarios) ? null : Comentarios,
                Estado = EstadoPago.PendienteRevision
            };

            _context.Pagos.Add(pago);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Pago informado correctamente. Quedo pendiente de revision.";
            return RedirectToAction(nameof(Details), new { id = pago.Id });
        }

        private async Task LoadExpensasPendientesAsync()
        {
            var email = HttpContext.Session.GetString("UserEmail");
            ViewBag.Expensas = await _context.Expensas
                .Include(e => e.UnidadFuncional)
                    .ThenInclude(u => u.Consorcio)
                .Where(e => e.Estado != EstadoExpensa.Pagada
                    && e.UnidadFuncional.MailPropietario == email)
                .OrderByDescending(e => e.FechaEmision)
                .ToListAsync();
        }

        private async Task<string> SaveComprobanteAsync(IFormFile? comprobante)
        {
            if (comprobante is null || comprobante.Length == 0)
            {
                return string.Empty;
            }

            var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads", "pagos");
            Directory.CreateDirectory(uploadsPath);

            var extension = Path.GetExtension(comprobante.FileName).ToLowerInvariant();
            var fileName = $"{Guid.NewGuid():N}{extension}";
            var filePath = Path.Combine(uploadsPath, fileName);

            await using var stream = System.IO.File.Create(filePath);
            await comprobante.CopyToAsync(stream);

            return $"/uploads/pagos/{fileName}";
        }

        private IActionResult RedirectToLocalOrDetails(RevisarPagoViewModel model)
        {
            if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }

            return RedirectToAction(nameof(Details), new { id = model.PagoId });
        }

        private string GetSafeReturnUrl(string? returnUrl, bool esPropietario)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return returnUrl;
            }

            return esPropietario
                ? Url.Action(nameof(MisPagos), "Pagos") ?? "/Pagos/MisPagos"
                : Url.Action(nameof(Index), "Pagos") ?? "/Pagos";
        }
    }
}
