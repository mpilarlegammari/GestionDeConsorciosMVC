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
