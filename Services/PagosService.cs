using GestionDeConsorciosMVC.Context;
using GestionDeConsorciosMVC.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace GestionDeConsorciosMVC.Services
{
    public class PagosService : IPagosService
    {
        private readonly GestionDeConsorciosContext _context;

        public PagosService(GestionDeConsorciosContext context)
        {
            _context = context;
        }

        public async Task<PagosAdminIndexViewModel> GetAdminIndexAsync(
            EstadoPago? estado = null,
            int? consorcioId = null,
            string? periodo = null)
        {
            var normalizedPeriodo = NormalizeOptional(periodo);
            var query = _context.Pagos
                .Include(pago => pago.Expensa)
                    .ThenInclude(expensa => expensa.UnidadFuncional)
                        .ThenInclude(unidad => unidad.Consorcio)
                .AsNoTracking();

            if (estado.HasValue)
            {
                query = query.Where(pago => pago.Estado == estado.Value);
            }

            if (consorcioId.HasValue)
            {
                query = query.Where(pago => pago.Expensa.UnidadFuncional.ConsorcioId == consorcioId.Value);
            }

            if (!string.IsNullOrWhiteSpace(normalizedPeriodo))
            {
                query = query.Where(pago => pago.Expensa.Periodo == normalizedPeriodo);
            }

            var pagos = await query
                .OrderBy(pago => pago.Estado)
                .ThenByDescending(pago => pago.FechaCreacion)
                .ToListAsync();

            return new PagosAdminIndexViewModel
            {
                Pagos = pagos,
                Consorcios = await GetConsorciosAsync(),
                Estado = estado,
                ConsorcioId = consorcioId,
                Periodo = normalizedPeriodo
            };
        }

        public async Task<MisPagosViewModel> GetMisPagosAsync(
            string email,
            int? unidadFuncionalId = null,
            string? periodo = null,
            int? anio = null,
            EstadoPago? estado = null,
            string? medioPago = null)
        {
            var normalizedEmail = Normalize(email);
            var normalizedPeriodo = NormalizeOptional(periodo);
            var normalizedMedioPago = NormalizeOptional(medioPago);
            var unidades = await GetOwnerUnidadesAsync(normalizedEmail);
            var unidadesSeleccionadas = unidadFuncionalId.HasValue
                ? unidades.Where(unidad => unidad.Id == unidadFuncionalId.Value).ToList()
                : unidades.ToList();
            var ownerUnidadIds = unidades.Select(unidad => unidad.Id).ToList();
            var selectedUnidadIds = unidadesSeleccionadas.Select(unidad => unidad.Id).ToList();

            var pagos = new List<Pago>();

            if (selectedUnidadIds.Count > 0)
            {
                var query = _context.Pagos
                    .Include(pago => pago.Expensa)
                        .ThenInclude(expensa => expensa.UnidadFuncional)
                            .ThenInclude(unidad => unidad.Consorcio)
                    .AsNoTracking()
                    .Where(pago => selectedUnidadIds.Contains(pago.Expensa.UnidadFuncionalId));

                if (!string.IsNullOrWhiteSpace(normalizedPeriodo))
                {
                    query = query.Where(pago => pago.Expensa.Periodo == normalizedPeriodo);
                }
                else if (anio.HasValue && anio.Value > 0)
                {
                    query = query.Where(pago => pago.FechaPago.Year == anio.Value);
                }

                if (estado.HasValue)
                {
                    query = query.Where(pago => pago.Estado == estado.Value);
                }

                if (!string.IsNullOrWhiteSpace(normalizedMedioPago))
                {
                    query = query.Where(pago => pago.MedioPago == normalizedMedioPago);
                }

                pagos = await query
                    .OrderByDescending(pago => pago.FechaPago)
                    .ThenByDescending(pago => pago.FechaCreacion)
                    .ToListAsync();
            }

            return new MisPagosViewModel
            {
                Pagos = pagos,
                UnidadesFuncionales = unidades,
                UnidadesSeleccionadas = unidadesSeleccionadas,
                PeriodosDisponibles = await GetPeriodosDisponiblesAsync(ownerUnidadIds),
                AniosDisponibles = await GetAniosDisponiblesAsync(ownerUnidadIds),
                MediosPagoDisponibles = await GetMediosPagoDisponiblesAsync(ownerUnidadIds),
                UnidadFuncionalId = unidadFuncionalId,
                Periodo = normalizedPeriodo,
                Anio = anio,
                Estado = estado,
                MedioPago = normalizedMedioPago
            };
        }

        public async Task<PagoDetailsViewModel?> GetDetailsAsync(int id)
        {
            var pago = await _context.Pagos
                .Include(item => item.Expensa)
                    .ThenInclude(expensa => expensa.UnidadFuncional)
                        .ThenInclude(unidad => unidad.Consorcio)
                .AsNoTracking()
                .FirstOrDefaultAsync(item => item.Id == id);

            return pago is null ? null : MapDetails(pago);
        }

        public async Task<bool> AprobarAsync(RevisarPagoViewModel model)
        {
            var pago = await _context.Pagos
                .Include(item => item.Expensa)
                .FirstOrDefaultAsync(item => item.Id == model.PagoId);

            if (pago is null)
            {
                return false;
            }

            pago.Estado = EstadoPago.Aprobado;
            pago.FechaRevision = DateTime.Now;
            pago.ObservacionAdministracion = NormalizeOptional(model.ObservacionAdministracion);
            pago.Expensa.Estado = EstadoExpensa.Pagada;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RechazarAsync(RevisarPagoViewModel model)
        {
            var pago = await _context.Pagos.FindAsync(model.PagoId);

            if (pago is null)
            {
                return false;
            }

            pago.Estado = EstadoPago.Rechazado;
            pago.FechaRevision = DateTime.Now;
            pago.ObservacionAdministracion = NormalizeOptional(model.ObservacionAdministracion);

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
            if (string.IsNullOrWhiteSpace(email))
            {
                return [];
            }

            return await _context.UnidadesFuncionales
                .Include(unidad => unidad.Consorcio)
                .AsNoTracking()
                .Where(unidad => unidad.MailPropietario == email)
                .OrderBy(unidad => unidad.Consorcio.Nombre)
                .ThenBy(unidad => unidad.NumeroUF)
                .ToListAsync();
        }

        private async Task<List<string>> GetPeriodosDisponiblesAsync(List<int> unidadIds)
        {
            if (unidadIds.Count == 0)
            {
                return [];
            }

            return await _context.Pagos
                .AsNoTracking()
                .Where(pago => unidadIds.Contains(pago.Expensa.UnidadFuncionalId))
                .Select(pago => pago.Expensa.Periodo)
                .Distinct()
                .OrderByDescending(periodo => periodo)
                .ToListAsync();
        }

        private async Task<List<int>> GetAniosDisponiblesAsync(List<int> unidadIds)
        {
            if (unidadIds.Count == 0)
            {
                return [DateTime.Today.Year];
            }

            var anios = await _context.Pagos
                .AsNoTracking()
                .Where(pago => unidadIds.Contains(pago.Expensa.UnidadFuncionalId))
                .Select(pago => pago.FechaPago.Year)
                .Distinct()
                .OrderByDescending(anio => anio)
                .ToListAsync();

            return anios.Count == 0 ? [DateTime.Today.Year] : anios;
        }

        private async Task<List<string>> GetMediosPagoDisponiblesAsync(List<int> unidadIds)
        {
            if (unidadIds.Count == 0)
            {
                return [];
            }

            return await _context.Pagos
                .AsNoTracking()
                .Where(pago => unidadIds.Contains(pago.Expensa.UnidadFuncionalId))
                .Select(pago => pago.MedioPago)
                .Distinct()
                .OrderBy(medio => medio)
                .ToListAsync();
        }

        private static PagoDetailsViewModel MapDetails(Pago pago)
        {
            var unidad = pago.Expensa.UnidadFuncional;

            return new PagoDetailsViewModel
            {
                Id = pago.Id,
                ExpensaId = pago.ExpensaId,
                Periodo = pago.Expensa.Periodo,
                MontoExpensa = pago.Expensa.MontoTotal,
                EstadoExpensa = pago.Expensa.Estado,
                ConsorcioNombre = unidad.Consorcio.Nombre,
                UnidadFuncionalNumero = unidad.NumeroUF,
                PropietarioNombre = unidad.NombrePropietario,
                PropietarioMail = unidad.MailPropietario,
                FechaPago = pago.FechaPago,
                MontoPagado = pago.MontoPagado,
                MedioPago = pago.MedioPago,
                NumeroOperacion = pago.NumeroOperacion,
                BancoEntidad = pago.BancoEntidad,
                ComprobantePath = pago.ComprobantePath,
                Comentarios = pago.Comentarios,
                Estado = pago.Estado,
                FechaCreacion = pago.FechaCreacion,
                FechaRevision = pago.FechaRevision,
                ObservacionAdministracion = pago.ObservacionAdministracion
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
