using GestionDeConsorciosMVC.Context;
using GestionDeConsorciosMVC.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace GestionDeConsorciosMVC.Services
{
    public class ReclamosService : IReclamosService
    {
        private static readonly List<string> CategoriasDisponibles =
        [
            "Mantenimiento",
            "Limpieza",
            "Ruido",
            "Seguridad",
            "Administracion",
            "Otro"
        ];

        private readonly GestionDeConsorciosContext _context;

        public ReclamosService(GestionDeConsorciosContext context)
        {
            _context = context;
        }

        public async Task<ReclamosAdminIndexViewModel> GetAdminIndexAsync(
            EstadoReclamo? estado = null,
            int? consorcioId = null,
            string? busqueda = null)
        {
            var normalizedBusqueda = NormalizeOptional(busqueda);
            var query = _context.Reclamos
                .Include(reclamo => reclamo.UnidadFuncional)
                    .ThenInclude(unidad => unidad.Consorcio)
                .AsNoTracking();

            if (estado.HasValue)
            {
                query = query.Where(reclamo => reclamo.Estado == estado.Value);
            }

            if (consorcioId.HasValue)
            {
                query = query.Where(reclamo => reclamo.UnidadFuncional.ConsorcioId == consorcioId.Value);
            }

            if (!string.IsNullOrWhiteSpace(normalizedBusqueda))
            {
                var pattern = $"%{normalizedBusqueda}%";
                query = query.Where(reclamo =>
                    EF.Functions.Like(reclamo.Asunto, pattern)
                    || EF.Functions.Like(reclamo.Categoria, pattern)
                    || EF.Functions.Like(reclamo.Descripcion, pattern)
                    || (reclamo.ObservacionAdministracion != null
                        && EF.Functions.Like(reclamo.ObservacionAdministracion, pattern)));
            }

            var reclamos = await query
                .OrderBy(reclamo => reclamo.Estado == EstadoReclamo.Cerrado)
                .ThenByDescending(reclamo => reclamo.FechaCreacion)
                .ToListAsync();

            return new ReclamosAdminIndexViewModel
            {
                Reclamos = reclamos,
                Consorcios = await GetConsorciosAsync(),
                Estado = estado,
                ConsorcioId = consorcioId,
                Busqueda = normalizedBusqueda
            };
        }

        public async Task<ReclamoCreateViewModel> BuildCreateViewModelAsync(string email, ReclamoCreateViewModel? model = null)
        {
            var viewModel = model ?? new ReclamoCreateViewModel();
            viewModel.UnidadesFuncionales = await GetOwnerUnidadesAsync(email);
            viewModel.Categorias = CategoriasDisponibles;

            if (viewModel.UnidadFuncionalId == 0 && viewModel.UnidadesFuncionales.Count == 1)
            {
                viewModel.UnidadFuncionalId = viewModel.UnidadesFuncionales[0].Id;
            }

            return viewModel;
        }

        public async Task<MisReclamosViewModel> GetMisReclamosAsync(string email, EstadoReclamo? estado = null, string? busqueda = null)
        {
            var normalizedEmail = Normalize(email);
            var normalizedBusqueda = NormalizeOptional(busqueda);
            var unidades = await GetOwnerUnidadesAsync(normalizedEmail);
            var unidadIds = unidades.Select(unidad => unidad.Id).ToList();

            var query = _context.Reclamos
                .Include(reclamo => reclamo.UnidadFuncional)
                    .ThenInclude(unidad => unidad.Consorcio)
                .AsNoTracking()
                .Where(reclamo => unidadIds.Contains(reclamo.UnidadFuncionalId));

            if (estado.HasValue)
            {
                query = query.Where(reclamo => reclamo.Estado == estado.Value);
            }

            if (!string.IsNullOrWhiteSpace(normalizedBusqueda))
            {
                var pattern = $"%{normalizedBusqueda}%";
                query = query.Where(reclamo =>
                    EF.Functions.Like(reclamo.Asunto, pattern)
                    || EF.Functions.Like(reclamo.Categoria, pattern)
                    || EF.Functions.Like(reclamo.Descripcion, pattern)
                    || (reclamo.ObservacionAdministracion != null
                        && EF.Functions.Like(reclamo.ObservacionAdministracion, pattern)));
            }

            return new MisReclamosViewModel
            {
                Reclamos = await query
                    .OrderBy(reclamo => reclamo.Estado == EstadoReclamo.Cerrado)
                    .ThenByDescending(reclamo => reclamo.FechaCreacion)
                    .ToListAsync(),
                UnidadesFuncionales = unidades,
                Estado = estado,
                Busqueda = normalizedBusqueda
            };
        }

        public async Task<ReclamoDetailsViewModel?> GetDetailsAsync(int id)
        {
            var reclamo = await _context.Reclamos
                .Include(item => item.UnidadFuncional)
                    .ThenInclude(unidad => unidad.Consorcio)
                .AsNoTracking()
                .FirstOrDefaultAsync(item => item.Id == id);

            return reclamo is null ? null : MapDetails(reclamo);
        }

        public async Task<bool> OwnerCanAccessAsync(int reclamoId, string email)
        {
            var normalizedEmail = Normalize(email);

            return await _context.Reclamos
                .AnyAsync(reclamo => reclamo.Id == reclamoId
                    && reclamo.UnidadFuncional.MailPropietario == normalizedEmail);
        }

        public async Task<bool> OwnerHasUnidadesAsync(string email)
        {
            var normalizedEmail = Normalize(email);

            return await _context.UnidadesFuncionales
                .AnyAsync(unidad => unidad.MailPropietario == normalizedEmail);
        }

        public async Task<bool> UnidadFuncionalBelongsToOwnerAsync(int unidadFuncionalId, string email)
        {
            var normalizedEmail = Normalize(email);

            return await _context.UnidadesFuncionales
                .AnyAsync(unidad => unidad.Id == unidadFuncionalId
                    && unidad.MailPropietario == normalizedEmail);
        }

        public async Task<Reclamo> CreateAsync(ReclamoCreateViewModel model)
        {
            var reclamo = new Reclamo
            {
                UnidadFuncionalId = model.UnidadFuncionalId,
                Asunto = Normalize(model.Asunto),
                Categoria = Normalize(model.Categoria),
                Descripcion = Normalize(model.Descripcion),
                Estado = EstadoReclamo.Abierto,
                FechaCreacion = DateTime.Now
            };

            _context.Reclamos.Add(reclamo);
            await _context.SaveChangesAsync();

            return reclamo;
        }

        public async Task<bool> CambiarEstadoAsync(CambiarEstadoReclamoViewModel model)
        {
            var reclamo = await _context.Reclamos.FindAsync(model.ReclamoId);

            if (reclamo is null)
            {
                return false;
            }

            reclamo.Estado = model.Estado;
            reclamo.ObservacionAdministracion = NormalizeOptional(model.ObservacionAdministracion);
            reclamo.FechaCierre = model.Estado == EstadoReclamo.Cerrado ? DateTime.Now : null;

            await _context.SaveChangesAsync();
            return true;
        }

        private async Task<List<Consorcio>> GetConsorciosAsync()
        {
            return await _context.Consorcios
                .AsNoTracking()
                .OrderBy(consorcio => consorcio.Nombre)
                .ToListAsync();
        }

        private async Task<List<UnidadFuncional>> GetOwnerUnidadesAsync(string email)
        {
            var normalizedEmail = Normalize(email);

            if (string.IsNullOrWhiteSpace(normalizedEmail))
            {
                return [];
            }

            return await _context.UnidadesFuncionales
                .Include(unidad => unidad.Consorcio)
                .AsNoTracking()
                .Where(unidad => unidad.MailPropietario == normalizedEmail)
                .OrderBy(unidad => unidad.Consorcio.Nombre)
                .ThenBy(unidad => unidad.NumeroUF)
                .ToListAsync();
        }

        private static ReclamoDetailsViewModel MapDetails(Reclamo reclamo)
        {
            var unidad = reclamo.UnidadFuncional;

            return new ReclamoDetailsViewModel
            {
                Id = reclamo.Id,
                UnidadFuncionalId = reclamo.UnidadFuncionalId,
                UnidadFuncionalNumero = unidad.NumeroUF,
                Piso = unidad.Piso,
                Departamento = unidad.Departamento,
                PropietarioNombre = unidad.NombrePropietario,
                PropietarioMail = unidad.MailPropietario,
                ConsorcioId = unidad.ConsorcioId,
                ConsorcioNombre = unidad.Consorcio.Nombre,
                Asunto = reclamo.Asunto,
                Categoria = reclamo.Categoria,
                Descripcion = reclamo.Descripcion,
                Estado = reclamo.Estado,
                FechaCreacion = reclamo.FechaCreacion,
                FechaCierre = reclamo.FechaCierre,
                ObservacionAdministracion = reclamo.ObservacionAdministracion
            };
        }

        private static string Normalize(string? value)
        {
            return (value ?? string.Empty).Trim();
        }

        private static string? NormalizeOptional(string? value)
        {
            var normalized = Normalize(value);
            return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
        }
    }
}
