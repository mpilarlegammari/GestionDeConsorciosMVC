using GestionDeConsorciosMVC.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace GestionDeConsorciosMVC.Controllers
{
    public class GastosController : Controller
    {
        private static readonly string[] AllowedExtensions = [".pdf", ".jpg", ".jpeg", ".png"];
        private readonly GestionDeConsorciosContext _context;
        private readonly IWebHostEnvironment _environment;

        public GastosController(GestionDeConsorciosContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var gastos = await _context.Gastos
                .Include(g => g.Consorcio)
                .OrderByDescending(g => g.Fecha)
                .ThenByDescending(g => g.Id)
                .ToListAsync();

            ViewBag.Consorcios = await _context.Consorcios.OrderBy(c => c.Nombre).ToListAsync();
            return View(gastos);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadConsorciosAsync();
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> MisGastos()
        {
            var gastos = await _context.Gastos
                .Include(g => g.Consorcio)
                .OrderByDescending(g => g.Fecha)
                .ToListAsync();

            return View(gastos);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var gasto = await _context.Gastos
                .Include(g => g.Consorcio)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (gasto is null)
            {
                return NotFound();
            }

            return View(gasto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            int ConsorcioId,
            string NumeroFactura,
            DateTime Fecha,
            decimal Monto,
            string Categoria,
            string Concepto,
            string? Descripcion,
            IFormFile? ArchivoFactura)
        {
            NumeroFactura = NumeroFactura?.Trim() ?? string.Empty;
            Concepto = Concepto?.Trim() ?? string.Empty;
            Categoria = Categoria?.Trim() ?? string.Empty;
            Descripcion = Descripcion?.Trim();

            if (!await _context.Consorcios.AnyAsync(c => c.Id == ConsorcioId))
            {
                ModelState.AddModelError(nameof(ConsorcioId), "Debe seleccionar un consorcio valido.");
            }

            if (string.IsNullOrWhiteSpace(NumeroFactura))
            {
                ModelState.AddModelError(nameof(NumeroFactura), "El numero de factura es obligatorio.");
            }
            else if (await _context.Gastos.AnyAsync(g => g.NumeroFactura == NumeroFactura))
            {
                ModelState.AddModelError(nameof(NumeroFactura), "Ya existe un gasto con ese numero de factura.");
            }

            if (Fecha == default)
            {
                ModelState.AddModelError(nameof(Fecha), "La fecha es obligatoria.");
            }

            if (Monto <= 0)
            {
                ModelState.AddModelError(nameof(Monto), "El monto debe ser mayor a 0.");
            }

            if (!Enum.TryParse<CategoriaGasto>(Categoria, out var categoriaGasto) || !Enum.IsDefined(categoriaGasto))
            {
                ModelState.AddModelError(nameof(Categoria), "Debe seleccionar una categoria valida.");
            }

            if (string.IsNullOrWhiteSpace(Concepto))
            {
                ModelState.AddModelError(nameof(Concepto), "El concepto es obligatorio.");
            }

            if (ArchivoFactura is not null && ArchivoFactura.Length > 0)
            {
                var extension = Path.GetExtension(ArchivoFactura.FileName).ToLowerInvariant();

                if (!AllowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError(nameof(ArchivoFactura), "Formato permitido: pdf, jpg, jpeg o png.");
                }
            }

            if (!ModelState.IsValid)
            {
                TempData["Error"] = string.Join(" ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                await LoadConsorciosAsync();
                return View();
            }

            var archivoPath = await SaveArchivoFacturaAsync(ArchivoFactura);
            var gasto = new Gasto
            {
                ConsorcioId = ConsorcioId,
                NumeroFactura = NumeroFactura,
                Fecha = Fecha,
                Monto = Monto,
                Categoria = categoriaGasto,
                Concepto = Concepto,
                Descripcion = string.IsNullOrWhiteSpace(Descripcion) ? null : Descripcion,
                ArchivoFacturaPath = archivoPath
            };

            _context.Gastos.Add(gasto);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Gasto registrado correctamente.";
            return RedirectToAction(nameof(Details), new { id = gasto.Id });
        }

        private async Task LoadConsorciosAsync()
        {
            ViewBag.Consorcios = await _context.Consorcios
                .OrderBy(c => c.Nombre)
                .ToListAsync();
        }

        private async Task<string?> SaveArchivoFacturaAsync(IFormFile? archivo)
        {
            if (archivo is null || archivo.Length == 0)
            {
                return null;
            }

            var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads", "gastos");
            Directory.CreateDirectory(uploadsPath);

            var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
            var fileName = $"{Guid.NewGuid():N}{extension}";
            var filePath = Path.Combine(uploadsPath, fileName);

            await using var stream = System.IO.File.Create(filePath);
            await archivo.CopyToAsync(stream);

            return $"/uploads/gastos/{fileName}";
        }
    }
}
