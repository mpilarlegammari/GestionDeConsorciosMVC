using GestionDeConsorciosMVC.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace GestionDeConsorciosMVC.Controllers
{
    public class PagosController : Controller
    {
        private static readonly string[] AllowedExtensions = [".pdf", ".jpg", ".jpeg", ".png"];
        private readonly GestionDeConsorciosContext _context;
        private readonly IWebHostEnvironment _environment;

        public PagosController(GestionDeConsorciosContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
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
        public async Task<IActionResult> MisPagos()
        {
            var email = HttpContext.Session.GetString("UserEmail");

            if (string.IsNullOrWhiteSpace(email))
            {
                return RedirectToAction("Login", "Auth");
            }

            var pagos = await _context.Pagos
                .Include(p => p.Expensa)
                    .ThenInclude(e => e.UnidadFuncional)
                        .ThenInclude(u => u.Consorcio)
                .Where(p => p.Expensa.UnidadFuncional.MailPropietario == email)
                .OrderByDescending(p => p.FechaPago)
                .ToListAsync();

            return View(pagos);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var role = HttpContext.Session.GetString("UserRole");
            var email = HttpContext.Session.GetString("UserEmail");
            var pago = await _context.Pagos
                .Include(p => p.Expensa)
                    .ThenInclude(e => e.UnidadFuncional)
                        .ThenInclude(u => u.Consorcio)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pago is null)
            {
                return NotFound();
            }

            if (role?.Equals("Propietario", StringComparison.OrdinalIgnoreCase) == true
                && !string.Equals(pago.Expensa.UnidadFuncional.MailPropietario, email, StringComparison.OrdinalIgnoreCase))
            {
                return Forbid();
            }

            return View(pago);
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
    }
}
