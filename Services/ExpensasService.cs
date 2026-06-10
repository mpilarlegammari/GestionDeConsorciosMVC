using GestionDeConsorciosMVC.Context;
using GestionDeConsorciosMVC.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace GestionDeConsorciosMVC.Services
{
    public class ExpensasService : IExpensasService
    {
        private readonly GestionDeConsorciosContext _context;

        public ExpensasService(GestionDeConsorciosContext context)
        {
            _context = context;
        }

        public async Task<ExpensasIndexViewModel> GetIndexAsync(
            int? consorcioId = null,
            string? periodo = null,
            int? mes = null,
            int? anio = null,
            EstadoExpensa? estado = null)
        {
            var normalizedPeriodo = NormalizeOptional(periodo);
            var query = _context.Expensas
                .Include(expensa => expensa.UnidadFuncional)
                    .ThenInclude(unidad => unidad.Consorcio)
                .Include(expensa => expensa.Pagos)
                .AsNoTracking();

            if (consorcioId.HasValue)
            {
                query = query.Where(expensa => expensa.UnidadFuncional.ConsorcioId == consorcioId.Value);
            }

            if (!string.IsNullOrWhiteSpace(normalizedPeriodo))
            {
                query = query.Where(expensa => expensa.Periodo == normalizedPeriodo);
            }
            else if (mes is >= 1 and <= 12 && anio.HasValue && anio.Value > 0)
            {
                var periodoDesdeMesAnio = $"{anio.Value:D4}-{mes.Value:D2}";
                query = query.Where(expensa => expensa.Periodo == periodoDesdeMesAnio);
            }
            else if (anio.HasValue && anio.Value > 0)
            {
                query = query.Where(expensa => expensa.FechaEmision.Year == anio.Value);
            }

            if (estado.HasValue)
            {
                query = query.Where(expensa => expensa.Estado == estado.Value);
            }

            var expensas = await query
                .OrderByDescending(expensa => expensa.FechaEmision)
                .ThenBy(expensa => expensa.UnidadFuncional.NumeroUF)
                .ToListAsync();

            return new ExpensasIndexViewModel
            {
                Expensas = expensas,
                Consorcios = await GetConsorciosAsync(),
                ConsorcioId = consorcioId,
                Periodo = normalizedPeriodo,
                Mes = mes,
                Anio = anio,
                Estado = estado
            };
        }

        public async Task<MisExpensasViewModel> GetMisExpensasAsync(
            string email,
            int? unidadFuncionalId = null,
            string? periodo = null,
            int? anio = null,
            EstadoExpensa? estado = null)
        {
            var normalizedEmail = Normalize(email);
            var normalizedPeriodo = NormalizeOptional(periodo);
            var unidades = await GetOwnerUnidadesAsync(normalizedEmail);
            var unidadesSeleccionadas = unidadFuncionalId.HasValue
                ? unidades.Where(unidad => unidad.Id == unidadFuncionalId.Value).ToList()
                : unidades.ToList();
            var ownerUnidadIds = unidades.Select(unidad => unidad.Id).ToList();
            var selectedUnidadIds = unidadesSeleccionadas.Select(unidad => unidad.Id).ToList();

            var expensas = new List<Expensa>();

            if (selectedUnidadIds.Count > 0)
            {
                var query = _context.Expensas
                    .Include(expensa => expensa.UnidadFuncional)
                        .ThenInclude(unidad => unidad.Consorcio)
                    .Include(expensa => expensa.Pagos)
                    .AsNoTracking()
                    .Where(expensa => selectedUnidadIds.Contains(expensa.UnidadFuncionalId));

                if (!string.IsNullOrWhiteSpace(normalizedPeriodo))
                {
                    query = query.Where(expensa => expensa.Periodo == normalizedPeriodo);
                }
                else if (anio.HasValue && anio.Value > 0)
                {
                    query = query.Where(expensa => expensa.FechaEmision.Year == anio.Value);
                }

                if (estado.HasValue)
                {
                    query = query.Where(expensa => expensa.Estado == estado.Value);
                }

                expensas = await query
                    .OrderByDescending(expensa => expensa.FechaEmision)
                    .ThenByDescending(expensa => expensa.Periodo)
                    .ThenBy(expensa => expensa.UnidadFuncional.NumeroUF)
                    .ToListAsync();
            }

            return new MisExpensasViewModel
            {
                Expensas = expensas,
                UnidadesFuncionales = unidades,
                UnidadesSeleccionadas = unidadesSeleccionadas,
                PeriodosDisponibles = await GetPeriodosDisponiblesAsync(ownerUnidadIds),
                AniosDisponibles = await GetAniosDisponiblesAsync(ownerUnidadIds),
                UnidadFuncionalId = unidadFuncionalId,
                Periodo = normalizedPeriodo,
                Anio = anio,
                Estado = estado
            };
        }

        public async Task<GenerarExpensasViewModel> BuildGenerarViewModelAsync(GenerarExpensasViewModel? model = null)
        {
            model ??= new GenerarExpensasViewModel();
            Normalize(model);

            model.Consorcios = await GetConsorciosAsync();

            var consorcio = model.ConsorcioId > 0
                ? model.Consorcios.FirstOrDefault(item => item.Id == model.ConsorcioId)
                : null;

            model.CantidadUnidades = consorcio?.UnidadesFuncionales.Count ?? 0;
            model.Gastos = consorcio is null || string.IsNullOrWhiteSpace(model.Periodo)
                ? new List<Gasto>()
                : await GetGastosPeriodoAsync(model.ConsorcioId, model.Periodo);

            return model;
        }

        public async Task<ExpensaDetailsViewModel?> GetDetailsAsync(int id)
        {
            var expensa = await _context.Expensas
                .Include(item => item.UnidadFuncional)
                    .ThenInclude(unidad => unidad.Consorcio)
                .Include(item => item.Pagos)
                .AsNoTracking()
                .FirstOrDefaultAsync(item => item.Id == id);

            if (expensa is null)
            {
                return null;
            }

            return new ExpensaDetailsViewModel
            {
                Id = expensa.Id,
                Periodo = expensa.Periodo,
                FechaEmision = expensa.FechaEmision,
                FechaVencimiento = expensa.FechaVencimiento,
                MontoTotal = expensa.MontoTotal,
                Estado = expensa.Estado,
                Observaciones = expensa.Observaciones,
                ConsorcioId = expensa.UnidadFuncional.ConsorcioId,
                ConsorcioNombre = expensa.UnidadFuncional.Consorcio.Nombre,
                UnidadFuncionalId = expensa.UnidadFuncionalId,
                UnidadFuncionalNumero = expensa.UnidadFuncional.NumeroUF,
                PropietarioNombre = expensa.UnidadFuncional.NombrePropietario,
                PropietarioMail = expensa.UnidadFuncional.MailPropietario,
                Pagos = expensa.Pagos
                    .OrderByDescending(pago => pago.FechaPago)
                    .ToList(),
                Gastos = await GetGastosPeriodoAsync(expensa.UnidadFuncional.ConsorcioId, expensa.Periodo)
            };
        }

        public async Task<Consorcio?> GetConsorcioConUnidadesAsync(int consorcioId)
        {
            return await _context.Consorcios
                .Include(consorcio => consorcio.UnidadesFuncionales)
                .FirstOrDefaultAsync(consorcio => consorcio.Id == consorcioId);
        }

        public async Task<bool> ExistsPeriodoGeneradoAsync(int consorcioId, string periodo)
        {
            var normalizedPeriodo = Normalize(periodo);

            return await _context.Expensas.AnyAsync(expensa =>
                expensa.Periodo == normalizedPeriodo &&
                expensa.UnidadFuncional.ConsorcioId == consorcioId);
        }

        public async Task<List<Gasto>> GetGastosPeriodoAsync(int consorcioId, string periodo)
        {
            var normalizedPeriodo = Normalize(periodo);

            if (!TryGetPeriodoRange(normalizedPeriodo, out var inicio, out var fin))
            {
                return [];
            }

            return await _context.Gastos
                .Where(gasto => gasto.ConsorcioId == consorcioId && gasto.Fecha >= inicio && gasto.Fecha < fin)
                .OrderBy(gasto => gasto.Fecha)
                .ThenBy(gasto => gasto.NumeroFactura)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<int> GenerarAsync(GenerarExpensasViewModel model)
        {
            Normalize(model);

            var consorcio = await GetConsorcioConUnidadesAsync(model.ConsorcioId)
                ?? throw new InvalidOperationException("No se encontro el consorcio seleccionado.");
            var gastos = await GetGastosPeriodoAsync(model.ConsorcioId, model.Periodo);

            if (consorcio.UnidadesFuncionales.Count == 0 || gastos.Count == 0)
            {
                return 0;
            }

            var total = gastos.Sum(gasto => gasto.Monto);
            var montoPorUnidad = Math.Round(total / consorcio.UnidadesFuncionales.Count, 2);
            var observaciones = NormalizeOptional(model.Observaciones);

            foreach (var unidad in consorcio.UnidadesFuncionales)
            {
                _context.Expensas.Add(new Expensa
                {
                    UnidadFuncionalId = unidad.Id,
                    Periodo = model.Periodo,
                    FechaEmision = model.FechaEmision.Date,
                    FechaVencimiento = model.FechaVencimiento.Date,
                    MontoTotal = montoPorUnidad,
                    Estado = EstadoExpensa.Pendiente,
                    Observaciones = observaciones
                });
            }

            await _context.SaveChangesAsync();
            return consorcio.UnidadesFuncionales.Count;
        }

        private async Task<List<Consorcio>> GetConsorciosAsync()
        {
            return await _context.Consorcios
                .Include(consorcio => consorcio.UnidadesFuncionales)
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

            return await _context.Expensas
                .AsNoTracking()
                .Where(expensa => unidadIds.Contains(expensa.UnidadFuncionalId))
                .Select(expensa => expensa.Periodo)
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

            var anios = await _context.Expensas
                .AsNoTracking()
                .Where(expensa => unidadIds.Contains(expensa.UnidadFuncionalId))
                .Select(expensa => expensa.FechaEmision.Year)
                .Distinct()
                .OrderByDescending(anio => anio)
                .ToListAsync();

            return anios.Count == 0 ? [DateTime.Today.Year] : anios;
        }

        private static void Normalize(GenerarExpensasViewModel model)
        {
            model.Periodo = Normalize(model.Periodo);
            model.CriterioDistribucion = Normalize(model.CriterioDistribucion);
            model.Observaciones = NormalizeOptional(model.Observaciones);

            if (model.FechaEmision == default)
            {
                model.FechaEmision = DateTime.Today;
            }

            if (model.FechaVencimiento == default)
            {
                model.FechaVencimiento = DateTime.Today.AddDays(10);
            }

            if (string.IsNullOrWhiteSpace(model.CriterioDistribucion))
            {
                model.CriterioDistribucion = "Partes iguales por UF";
            }
        }

        private static bool TryGetPeriodoRange(string periodo, out DateTime inicio, out DateTime fin)
        {
            var parsed = DateTime.TryParseExact(
                $"{periodo}-01",
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out inicio);

            fin = parsed ? inicio.AddMonths(1) : default;
            return parsed;
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
