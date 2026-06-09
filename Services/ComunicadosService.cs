using GestionDeConsorciosMVC.Context;
using GestionDeConsorciosMVC.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace GestionDeConsorciosMVC.Services
{
    public class ComunicadosService : IComunicadosService
    {
        private readonly GestionDeConsorciosContext _context;

        public ComunicadosService(GestionDeConsorciosContext context)
        {
            _context = context;
        }

        public async Task<ComunicadosAdminIndexViewModel> GetAdminIndexAsync(
            int? consorcioId = null,
            bool? importante = null,
            string? busqueda = null)
        {
            var normalizedBusqueda = NormalizeOptional(busqueda);
            var query = _context.Comunicados
                .Include(comunicado => comunicado.Consorcio)
                .AsNoTracking();

            if (consorcioId.HasValue)
            {
                query = query.Where(comunicado => comunicado.ConsorcioId == consorcioId.Value);
            }

            if (importante.HasValue)
            {
                query = query.Where(comunicado => comunicado.Importante == importante.Value);
            }

            if (!string.IsNullOrWhiteSpace(normalizedBusqueda))
            {
                var pattern = $"%{normalizedBusqueda}%";
                query = query.Where(comunicado =>
                    EF.Functions.Like(comunicado.Titulo, pattern)
                    || EF.Functions.Like(comunicado.Mensaje, pattern));
            }

            var comunicados = await query
                .OrderByDescending(comunicado => comunicado.Importante)
                .ThenByDescending(comunicado => comunicado.FechaPublicacion)
                .ToListAsync();

            return new ComunicadosAdminIndexViewModel
            {
                Comunicados = comunicados,
                Consorcios = await GetConsorciosAsync(),
                ConsorcioId = consorcioId,
                Importante = importante,
                Busqueda = normalizedBusqueda
            };
        }

        public async Task<ComunicadoCreateViewModel> BuildCreateViewModelAsync(ComunicadoCreateViewModel? model = null)
        {
            var viewModel = model ?? new ComunicadoCreateViewModel();
            viewModel.Consorcios = await GetConsorciosAsync();
            return viewModel;
        }

        public async Task<ComunicadoDetailsViewModel?> GetDetailsAsync(int id)
        {
            var comunicado = await _context.Comunicados
                .Include(item => item.Consorcio)
                .AsNoTracking()
                .FirstOrDefaultAsync(item => item.Id == id);

            return comunicado is null ? null : MapDetails(comunicado);
        }

        public async Task<MisComunicadosViewModel> GetMisComunicadosAsync(string email, bool? importante = null, string? busqueda = null)
        {
            var normalizedEmail = Normalize(email);
            var normalizedBusqueda = NormalizeOptional(busqueda);

            if (string.IsNullOrWhiteSpace(normalizedEmail))
            {
                return new MisComunicadosViewModel
                {
                    Importante = importante,
                    Busqueda = normalizedBusqueda
                };
            }

            var consorcioIds = await _context.UnidadesFuncionales
                .AsNoTracking()
                .Where(unidad => unidad.MailPropietario == normalizedEmail)
                .Select(unidad => unidad.ConsorcioId)
                .Distinct()
                .ToListAsync();

            var consorciosNombres = await _context.Consorcios
                .AsNoTracking()
                .Where(consorcio => consorcioIds.Contains(consorcio.Id))
                .OrderBy(consorcio => consorcio.Nombre)
                .Select(consorcio => consorcio.Nombre)
                .ToListAsync();

            var query = _context.Comunicados
                .Include(comunicado => comunicado.Consorcio)
                .AsNoTracking()
                .Where(comunicado => consorcioIds.Contains(comunicado.ConsorcioId));

            if (importante.HasValue)
            {
                query = query.Where(comunicado => comunicado.Importante == importante.Value);
            }

            if (!string.IsNullOrWhiteSpace(normalizedBusqueda))
            {
                var pattern = $"%{normalizedBusqueda}%";
                query = query.Where(comunicado =>
                    EF.Functions.Like(comunicado.Titulo, pattern)
                    || EF.Functions.Like(comunicado.Mensaje, pattern));
            }

            return new MisComunicadosViewModel
            {
                Comunicados = await query
                    .OrderByDescending(comunicado => comunicado.Importante)
                    .ThenByDescending(comunicado => comunicado.FechaPublicacion)
                    .ToListAsync(),
                ConsorciosNombres = consorciosNombres,
                Importante = importante,
                Busqueda = normalizedBusqueda
            };
        }

        public async Task<bool> ConsorcioExistsAsync(int consorcioId)
        {
            return await _context.Consorcios.AnyAsync(consorcio => consorcio.Id == consorcioId);
        }

        public async Task<Comunicado> CreateAsync(ComunicadoCreateViewModel model, string? archivoAdjuntoPath)
        {
            var comunicado = new Comunicado
            {
                ConsorcioId = model.ConsorcioId,
                Titulo = Normalize(model.Titulo),
                Mensaje = Normalize(model.Mensaje),
                Importante = model.Importante,
                ArchivoAdjuntoPath = NormalizeOptional(archivoAdjuntoPath),
                FechaPublicacion = DateTime.Now
            };

            _context.Comunicados.Add(comunicado);
            await _context.SaveChangesAsync();

            return comunicado;
        }

        private async Task<List<Consorcio>> GetConsorciosAsync()
        {
            return await _context.Consorcios
                .AsNoTracking()
                .OrderBy(consorcio => consorcio.Nombre)
                .ToListAsync();
        }

        private static ComunicadoDetailsViewModel MapDetails(Comunicado comunicado)
        {
            return new ComunicadoDetailsViewModel
            {
                Id = comunicado.Id,
                ConsorcioId = comunicado.ConsorcioId,
                ConsorcioNombre = comunicado.Consorcio.Nombre,
                Titulo = comunicado.Titulo,
                Mensaje = comunicado.Mensaje,
                FechaPublicacion = comunicado.FechaPublicacion,
                ArchivoAdjuntoPath = comunicado.ArchivoAdjuntoPath,
                Importante = comunicado.Importante
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
