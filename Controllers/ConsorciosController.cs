using GestionDeConsorciosMVC.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace GestionDeConsorciosMVC.Controllers
{
    public class ConsorciosController : Controller
    {
        private readonly GestionDeConsorciosContext _context;

        public ConsorciosController(GestionDeConsorciosContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var consorcios = await _context.Consorcios
                .Include(c => c.UnidadesFuncionales)
                .OrderBy(c => c.Nombre)
                .ToListAsync();

            return View(consorcios);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var consorcio = await _context.Consorcios
                .Include(c => c.UnidadesFuncionales)
                    .ThenInclude(u => u.Expensas)
                .Include(c => c.UnidadesFuncionales)
                    .ThenInclude(u => u.Reclamos)
                .Include(c => c.Gastos)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (consorcio is null)
            {
                return NotFound();
            }

            return View(consorcio);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IFormCollection form)
        {
            var nombre = form["nombreConsorcio"].ToString().Trim();
            var cuit = form["cuit"].ToString().Trim();
            var direccion = form["direccion"].ToString().Trim();
            var ciudad = form["ciudad"].ToString().Trim();
            var codigoPostal = form["codigoPostal"].ToString().Trim();
            var observaciones = form["observaciones"].ToString().Trim();
            var cantidadPisosValue = form["cantidadPisos"].ToString();

            if (string.IsNullOrWhiteSpace(nombre))
            {
                ModelState.AddModelError(nameof(nombre), "El nombre del consorcio es obligatorio.");
            }

            if (string.IsNullOrWhiteSpace(cuit))
            {
                ModelState.AddModelError(nameof(cuit), "El CUIT es obligatorio.");
            }

            if (string.IsNullOrWhiteSpace(direccion))
            {
                ModelState.AddModelError(nameof(direccion), "La direccion es obligatoria.");
            }

            if (string.IsNullOrWhiteSpace(ciudad))
            {
                ModelState.AddModelError(nameof(ciudad), "La ciudad es obligatoria.");
            }

            if (string.IsNullOrWhiteSpace(codigoPostal))
            {
                ModelState.AddModelError(nameof(codigoPostal), "El codigo postal es obligatorio.");
            }

            if (!int.TryParse(cantidadPisosValue, out var cantidadPisos) || cantidadPisos <= 0)
            {
                ModelState.AddModelError(nameof(cantidadPisos), "La cantidad de pisos debe ser mayor a 0.");
            }

            var numerosUf = form["ufNumero"].Select(value => value?.Trim() ?? string.Empty).ToList();
            var ufsDuplicadas = numerosUf
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .GroupBy(value => value, StringComparer.OrdinalIgnoreCase)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key)
                .ToList();

            if (ufsDuplicadas.Count > 0)
            {
                ModelState.AddModelError("ufNumero", $"Hay unidades funcionales duplicadas: {string.Join(", ", ufsDuplicadas)}.");
            }

            if (!numerosUf.Any(value => !string.IsNullOrWhiteSpace(value)))
            {
                ModelState.AddModelError("ufNumero", "Debe cargar al menos una unidad funcional.");
            }

            if (!ModelState.IsValid)
            {
                TempData["Error"] = string.Join(" ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return View();
            }

            var consorcio = new Consorcio
            {
                Nombre = nombre,
                Cuit = cuit,
                Direccion = direccion,
                Ciudad = ciudad,
                CodigoPostal = codigoPostal,
                CantidadPisos = cantidadPisos,
                Observaciones = string.IsNullOrWhiteSpace(observaciones) ? null : observaciones,
                Estado = EstadoConsorcio.Activo
            };

            var pisos = form["ufPiso"];
            var departamentos = form["ufDepartamento"];
            var propietarios = form["ufPropietario"];
            var mails = form["ufMail"];
            var dnis = form["ufDni"];
            var telefonos = form["ufTelefono"];

            for (var i = 0; i < numerosUf.Count; i++)
            {
                var numeroUf = numerosUf[i];

                if (string.IsNullOrWhiteSpace(numeroUf))
                {
                    continue;
                }

                consorcio.UnidadesFuncionales.Add(new UnidadFuncional
                {
                    NumeroUF = numeroUf,
                    Piso = i < pisos.Count ? pisos[i]?.Trim() ?? string.Empty : string.Empty,
                    Departamento = i < departamentos.Count ? departamentos[i]?.Trim() ?? string.Empty : string.Empty,
                    NombrePropietario = i < propietarios.Count ? propietarios[i]?.Trim() ?? string.Empty : string.Empty,
                    MailPropietario = i < mails.Count ? mails[i]?.Trim() ?? string.Empty : string.Empty,
                    DniPropietario = i < dnis.Count ? dnis[i]?.Trim() ?? string.Empty : string.Empty,
                    Telefono = i < telefonos.Count ? telefonos[i]?.Trim() : null,
                    Estado = EstadoUnidadFuncional.Activa
                });
            }

            _context.Consorcios.Add(consorcio);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Consorcio creado correctamente.";
            return RedirectToAction(nameof(Details), new { id = consorcio.Id });
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
