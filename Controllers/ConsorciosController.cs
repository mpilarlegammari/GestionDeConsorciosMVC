using GestionDeConsorciosMVC.Context;
using GestionDeConsorciosMVC.Services;
using GestionDeConsorciosMVC.ViewModels;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace GestionDeConsorciosMVC.Controllers
{
    public class ConsorciosController : Controller
    {
        private readonly GestionDeConsorciosContext _context;
        private readonly IUsuariosService _usuariosService;

        public ConsorciosController(GestionDeConsorciosContext context, IUsuariosService usuariosService)
        {
            _context = context;
            _usuariosService = usuariosService;
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
                .Include(c => c.Amenities)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (consorcio is null)
            {
                return NotFound();
            }

            return View(MapDetailsViewModel(consorcio));
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new ConsorcioViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ConsorcioViewModel model)
        {
            Normalize(model);
            ValidateUnidadesFuncionales(model);
            ValidateAmenities(model);

            if (!ModelState.IsValid)
            {
                EnsureRows(model);
                return View(model);
            }

            var consorcio = new Consorcio
            {
                Nombre = model.Nombre,
                Cuit = model.Cuit,
                Direccion = model.Direccion,
                Ciudad = model.Ciudad,
                CodigoPostal = model.CodigoPostal,
                CantidadPisos = model.CantidadPisos,
                Observaciones = model.Observaciones,
                Estado = EstadoConsorcio.Activo
            };

            foreach (var unidad in model.UnidadesFuncionales)
            {
                consorcio.UnidadesFuncionales.Add(MapUnidadFuncional(unidad));
            }

            foreach (var amenity in model.Amenities.Where(HasAmenityData))
            {
                consorcio.Amenities.Add(MapAmenity(amenity));
            }

            _context.Consorcios.Add(consorcio);
            await _context.SaveChangesAsync();
            await _usuariosService.EnsurePropietarioUsersAsync(consorcio.UnidadesFuncionales);

            TempData["Success"] = "Consorcio creado correctamente.";
            return RedirectToAction(nameof(Details), new { id = consorcio.Id });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var consorcio = await _context.Consorcios
                .Include(c => c.UnidadesFuncionales)
                .Include(c => c.Amenities)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (consorcio is null)
            {
                return NotFound();
            }

            return View(MapEditViewModel(consorcio));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ConsorcioViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            Normalize(model);
            ValidateUnidadesFuncionales(model);
            ValidateAmenities(model);

            if (!ModelState.IsValid)
            {
                EnsureRows(model);
                return View(model);
            }

            var consorcio = await _context.Consorcios
                .Include(c => c.UnidadesFuncionales)
                .Include(c => c.Amenities)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (consorcio is null)
            {
                return NotFound();
            }

            consorcio.Nombre = model.Nombre;
            consorcio.Cuit = model.Cuit;
            consorcio.Direccion = model.Direccion;
            consorcio.Ciudad = model.Ciudad;
            consorcio.CodigoPostal = model.CodigoPostal;
            consorcio.CantidadPisos = model.CantidadPisos;
            consorcio.Observaciones = model.Observaciones;
            consorcio.Estado = model.Estado;

            foreach (var unidadModel in model.UnidadesFuncionales)
            {
                var unidad = unidadModel.Id > 0
                    ? consorcio.UnidadesFuncionales.FirstOrDefault(u => u.Id == unidadModel.Id)
                    : null;

                if (unidad is null && unidadModel.Id > 0)
                {
                    ModelState.AddModelError("", $"La unidad funcional {unidadModel.NumeroUF} no pertenece al consorcio.");
                    EnsureRows(model);
                    return View(model);
                }

                if (unidad is null)
                {
                    consorcio.UnidadesFuncionales.Add(MapUnidadFuncional(unidadModel));
                    continue;
                }

                UpdateUnidadFuncional(unidad, unidadModel);
            }

            foreach (var amenityModel in model.Amenities.Where(HasAmenityData))
            {
                var amenity = amenityModel.Id > 0
                    ? consorcio.Amenities.FirstOrDefault(a => a.Id == amenityModel.Id)
                    : null;

                if (amenity is null && amenityModel.Id > 0)
                {
                    ModelState.AddModelError("", $"El amenity {amenityModel.Nombre} no pertenece al consorcio.");
                    EnsureRows(model);
                    return View(model);
                }

                if (amenity is null)
                {
                    consorcio.Amenities.Add(MapAmenity(amenityModel));
                    continue;
                }

                UpdateAmenity(amenity, amenityModel);
            }

            await _context.SaveChangesAsync();
            await _usuariosService.EnsurePropietarioUsersAsync(consorcio.UnidadesFuncionales);

            TempData["Success"] = "Consorcio actualizado correctamente.";
            return RedirectToAction(nameof(Details), new { id = consorcio.Id });
        }

        private void ValidateUnidadesFuncionales(ConsorcioViewModel model)
        {
            model.UnidadesFuncionales ??= new List<UnidadFuncionalViewModel>();

            if (model.UnidadesFuncionales.Count == 0)
            {
                ModelState.AddModelError(nameof(model.UnidadesFuncionales), "Debe cargar al menos una unidad funcional.");
                return;
            }

            var emailValidator = new EmailAddressAttribute();
            var numerosUf = new List<(string Numero, int Index)>();

            for (var i = 0; i < model.UnidadesFuncionales.Count; i++)
            {
                var unidad = model.UnidadesFuncionales[i];

                if (string.IsNullOrWhiteSpace(unidad.NumeroUF))
                {
                    ModelState.AddModelError($"UnidadesFuncionales[{i}].NumeroUF", "El numero de UF es obligatorio.");
                }
                else
                {
                    numerosUf.Add((unidad.NumeroUF, i));
                }

                if (string.IsNullOrWhiteSpace(unidad.DniPropietario))
                {
                    ModelState.AddModelError($"UnidadesFuncionales[{i}].DniPropietario", "El DNI del propietario es obligatorio.");
                }

                if (!string.IsNullOrWhiteSpace(unidad.MailPropietario) && !emailValidator.IsValid(unidad.MailPropietario))
                {
                    ModelState.AddModelError($"UnidadesFuncionales[{i}].MailPropietario", "El email del propietario no tiene un formato valido.");
                }
            }

            var duplicados = numerosUf
                .GroupBy(uf => uf.Numero, StringComparer.OrdinalIgnoreCase)
                .Where(group => group.Count() > 1)
                .SelectMany(group => group)
                .ToList();

            foreach (var duplicado in duplicados)
            {
                ModelState.AddModelError($"UnidadesFuncionales[{duplicado.Index}].NumeroUF", "No pueden existir dos unidades funcionales con el mismo numero.");
            }
        }

        private void ValidateAmenities(ConsorcioViewModel model)
        {
            model.Amenities ??= new List<AmenityViewModel>();

            for (var i = 0; i < model.Amenities.Count; i++)
            {
                var amenity = model.Amenities[i];

                if (!HasAmenityData(amenity))
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(amenity.Nombre))
                {
                    ModelState.AddModelError($"Amenities[{i}].Nombre", "El nombre del amenity es obligatorio.");
                }

                if (amenity.Capacidad <= 0)
                {
                    ModelState.AddModelError($"Amenities[{i}].Capacidad", "La capacidad debe ser mayor a 0.");
                }
            }

            var duplicados = model.Amenities
                .Select((amenity, index) => new { amenity.Nombre, Index = index })
                .Where(item => !string.IsNullOrWhiteSpace(item.Nombre))
                .GroupBy(item => item.Nombre.Trim(), StringComparer.OrdinalIgnoreCase)
                .Where(group => group.Count() > 1)
                .SelectMany(group => group)
                .ToList();

            foreach (var duplicado in duplicados)
            {
                ModelState.AddModelError($"Amenities[{duplicado.Index}].Nombre", "No pueden existir dos amenities con el mismo nombre.");
            }
        }

        private static void Normalize(ConsorcioViewModel model)
        {
            model.Nombre = model.Nombre?.Trim() ?? string.Empty;
            model.Cuit = model.Cuit?.Trim() ?? string.Empty;
            model.Direccion = model.Direccion?.Trim() ?? string.Empty;
            model.Ciudad = model.Ciudad?.Trim() ?? string.Empty;
            model.CodigoPostal = model.CodigoPostal?.Trim() ?? string.Empty;
            model.Observaciones = string.IsNullOrWhiteSpace(model.Observaciones) ? null : model.Observaciones.Trim();
            model.UnidadesFuncionales ??= new List<UnidadFuncionalViewModel>();
            model.Amenities ??= new List<AmenityViewModel>();

            foreach (var unidad in model.UnidadesFuncionales)
            {
                unidad.NumeroUF = unidad.NumeroUF?.Trim() ?? string.Empty;
                unidad.Piso = unidad.Piso?.Trim() ?? string.Empty;
                unidad.Departamento = unidad.Departamento?.Trim() ?? string.Empty;
                unidad.NombrePropietario = unidad.NombrePropietario?.Trim() ?? string.Empty;
                unidad.MailPropietario = string.IsNullOrWhiteSpace(unidad.MailPropietario) ? null : unidad.MailPropietario.Trim();
                unidad.DniPropietario = unidad.DniPropietario?.Trim() ?? string.Empty;
                unidad.Telefono = string.IsNullOrWhiteSpace(unidad.Telefono) ? null : unidad.Telefono.Trim();
            }

            foreach (var amenity in model.Amenities)
            {
                amenity.Nombre = amenity.Nombre?.Trim() ?? string.Empty;
                amenity.Descripcion = string.IsNullOrWhiteSpace(amenity.Descripcion) ? null : amenity.Descripcion.Trim();
            }
        }

        private static void EnsureUnidadFuncionalRow(ConsorcioViewModel model)
        {
            if (model.UnidadesFuncionales.Count == 0)
            {
                model.UnidadesFuncionales.Add(new UnidadFuncionalViewModel());
            }
        }

        private static void EnsureAmenityRow(ConsorcioViewModel model)
        {
            if (model.Amenities.Count == 0)
            {
                model.Amenities.Add(new AmenityViewModel());
            }
        }

        private static void EnsureRows(ConsorcioViewModel model)
        {
            EnsureUnidadFuncionalRow(model);
            EnsureAmenityRow(model);
        }

        private static UnidadFuncional MapUnidadFuncional(UnidadFuncionalViewModel model)
        {
            return new UnidadFuncional
            {
                NumeroUF = model.NumeroUF,
                Piso = model.Piso,
                Departamento = model.Departamento,
                NombrePropietario = model.NombrePropietario,
                MailPropietario = model.MailPropietario ?? string.Empty,
                DniPropietario = model.DniPropietario,
                Telefono = model.Telefono,
                Estado = EstadoUnidadFuncional.Activa
            };
        }

        private static void UpdateUnidadFuncional(UnidadFuncional unidad, UnidadFuncionalViewModel model)
        {
            unidad.NumeroUF = model.NumeroUF;
            unidad.Piso = model.Piso;
            unidad.Departamento = model.Departamento;
            unidad.NombrePropietario = model.NombrePropietario;
            unidad.MailPropietario = model.MailPropietario ?? string.Empty;
            unidad.DniPropietario = model.DniPropietario;
            unidad.Telefono = model.Telefono;
        }

        private static Amenity MapAmenity(AmenityViewModel model)
        {
            return new Amenity
            {
                Nombre = model.Nombre,
                Descripcion = model.Descripcion,
                Capacidad = model.Capacidad,
                Activo = model.Activo
            };
        }

        private static void UpdateAmenity(Amenity amenity, AmenityViewModel model)
        {
            amenity.Nombre = model.Nombre;
            amenity.Descripcion = model.Descripcion;
            amenity.Capacidad = model.Capacidad;
            amenity.Activo = model.Activo;
        }

        private static bool HasAmenityData(AmenityViewModel amenity)
        {
            return amenity.Id > 0
                || !string.IsNullOrWhiteSpace(amenity.Nombre)
                || !string.IsNullOrWhiteSpace(amenity.Descripcion)
                || amenity.Capacidad != 1;
        }

        private static ConsorcioViewModel MapEditViewModel(Consorcio consorcio)
        {
            var model = new ConsorcioViewModel
            {
                Id = consorcio.Id,
                Nombre = consorcio.Nombre,
                Cuit = consorcio.Cuit,
                Direccion = consorcio.Direccion,
                Ciudad = consorcio.Ciudad,
                CodigoPostal = consorcio.CodigoPostal,
                CantidadPisos = consorcio.CantidadPisos,
                Observaciones = consorcio.Observaciones,
                Estado = consorcio.Estado,
                UnidadesFuncionales = consorcio.UnidadesFuncionales
                    .OrderBy(u => u.NumeroUF)
                    .Select(MapUnidadFuncionalViewModel)
                    .ToList(),
                Amenities = consorcio.Amenities
                    .OrderBy(a => a.Nombre)
                    .Select(MapAmenityViewModel)
                    .ToList()
            };

            EnsureRows(model);
            return model;
        }

        private static ConsorcioDetailsViewModel MapDetailsViewModel(Consorcio consorcio)
        {
            return new ConsorcioDetailsViewModel
            {
                Id = consorcio.Id,
                Nombre = consorcio.Nombre,
                Cuit = consorcio.Cuit,
                Direccion = consorcio.Direccion,
                Ciudad = consorcio.Ciudad,
                CodigoPostal = consorcio.CodigoPostal,
                CantidadPisos = consorcio.CantidadPisos,
                Observaciones = consorcio.Observaciones,
                Estado = consorcio.Estado,
                FechaCreacion = consorcio.FechaCreacion,
                CantidadUnidades = consorcio.UnidadesFuncionales.Count,
                ExpensasPendientes = consorcio.UnidadesFuncionales
                    .SelectMany(u => u.Expensas)
                    .Count(e => e.Estado == EstadoExpensa.Pendiente),
                TotalGastos = consorcio.Gastos.Sum(g => g.Monto),
                CantidadGastos = consorcio.Gastos.Count,
                ReclamosAbiertos = consorcio.UnidadesFuncionales
                    .SelectMany(u => u.Reclamos)
                    .Count(r => r.Estado != EstadoReclamo.Cerrado),
                UnidadesFuncionales = consorcio.UnidadesFuncionales
                    .OrderBy(u => u.NumeroUF)
                    .Select(MapUnidadFuncionalViewModel)
                    .ToList(),
                Amenities = consorcio.Amenities
                    .OrderBy(a => a.Nombre)
                    .Select(MapAmenityViewModel)
                    .ToList()
            };
        }

        private static UnidadFuncionalViewModel MapUnidadFuncionalViewModel(UnidadFuncional unidad)
        {
            return new UnidadFuncionalViewModel
            {
                Id = unidad.Id,
                NumeroUF = unidad.NumeroUF,
                Piso = unidad.Piso,
                Departamento = unidad.Departamento,
                NombrePropietario = unidad.NombrePropietario,
                MailPropietario = unidad.MailPropietario,
                DniPropietario = unidad.DniPropietario,
                Telefono = unidad.Telefono,
                Estado = unidad.Estado
            };
        }

        private static AmenityViewModel MapAmenityViewModel(Amenity amenity)
        {
            return new AmenityViewModel
            {
                Id = amenity.Id,
                Nombre = amenity.Nombre,
                Descripcion = amenity.Descripcion,
                Capacidad = amenity.Capacidad,
                Activo = amenity.Activo
            };
        }
    }
}
