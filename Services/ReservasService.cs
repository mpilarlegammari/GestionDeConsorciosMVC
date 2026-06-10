using GestionDeConsorciosMVC.Context;
using GestionDeConsorciosMVC.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace GestionDeConsorciosMVC.Services
{
    public class ReservasService : IReservasService
    {
        private const int MaxReservasMensualesPorUf = 5;
        private readonly GestionDeConsorciosContext _context;

        public ReservasService(GestionDeConsorciosContext context)
        {
            _context = context;
        }

        public async Task<AmenitiesIndexViewModel> GetAmenitiesIndexAsync(int? consorcioId = null, bool? activo = null, string? busqueda = null)
        {
            var normalizedBusqueda = NormalizeOptional(busqueda);
            var query = _context.Amenities
                .Include(amenity => amenity.Consorcio)
                .AsNoTracking();

            if (consorcioId.HasValue)
            {
                query = query.Where(amenity => amenity.ConsorcioId == consorcioId.Value);
            }

            if (activo.HasValue)
            {
                query = query.Where(amenity => amenity.Activo == activo.Value);
            }

            if (!string.IsNullOrWhiteSpace(normalizedBusqueda))
            {
                var pattern = $"%{normalizedBusqueda}%";
                query = query.Where(amenity =>
                    EF.Functions.Like(amenity.Nombre, pattern)
                    || (amenity.Descripcion != null && EF.Functions.Like(amenity.Descripcion, pattern)));
            }

            return new AmenitiesIndexViewModel
            {
                Amenities = await query
                    .OrderBy(amenity => amenity.Consorcio.Nombre)
                    .ThenBy(amenity => amenity.Nombre)
                    .ToListAsync(),
                Consorcios = await GetConsorciosAsync(),
                ConsorcioId = consorcioId,
                Activo = activo,
                Busqueda = normalizedBusqueda
            };
        }

        public async Task<AmenityCreateViewModel> BuildAmenityCreateViewModelAsync(AmenityCreateViewModel? model = null)
        {
            var viewModel = model ?? new AmenityCreateViewModel();
            viewModel.Consorcios = await GetConsorciosAsync();
            return viewModel;
        }

        public async Task<bool> ConsorcioExistsAsync(int consorcioId)
        {
            return await _context.Consorcios.AnyAsync(consorcio => consorcio.Id == consorcioId);
        }

        public async Task<Amenity> CreateAmenityAsync(AmenityCreateViewModel model)
        {
            var amenity = new Amenity
            {
                ConsorcioId = model.ConsorcioId,
                Nombre = Normalize(model.Nombre),
                Descripcion = NormalizeOptional(model.Descripcion),
                Capacidad = model.Capacidad,
                Activo = model.Activo
            };

            _context.Amenities.Add(amenity);
            await _context.SaveChangesAsync();

            return amenity;
        }

        public async Task<ReservasAdminIndexViewModel> GetAdminIndexAsync(
            EstadoReserva? estado = null,
            int? consorcioId = null,
            DateTime? fecha = null,
            string? busqueda = null)
        {
            var normalizedBusqueda = NormalizeOptional(busqueda);
            var query = _context.Reservas
                .Include(reserva => reserva.Amenity)
                    .ThenInclude(amenity => amenity.Consorcio)
                .Include(reserva => reserva.UnidadFuncional)
                .AsNoTracking();

            if (estado.HasValue)
            {
                query = query.Where(reserva => reserva.Estado == estado.Value);
            }

            if (consorcioId.HasValue)
            {
                query = query.Where(reserva => reserva.Amenity.ConsorcioId == consorcioId.Value);
            }

            if (fecha.HasValue)
            {
                var selectedDate = fecha.Value.Date;
                query = query.Where(reserva => reserva.FechaReserva.Date == selectedDate);
            }

            if (!string.IsNullOrWhiteSpace(normalizedBusqueda))
            {
                var pattern = $"%{normalizedBusqueda}%";
                query = query.Where(reserva =>
                    EF.Functions.Like(reserva.Amenity.Nombre, pattern)
                    || EF.Functions.Like(reserva.UnidadFuncional.NumeroUF, pattern)
                    || EF.Functions.Like(reserva.UnidadFuncional.NombrePropietario, pattern)
                    || (reserva.Observaciones != null && EF.Functions.Like(reserva.Observaciones, pattern)));
            }

            return new ReservasAdminIndexViewModel
            {
                Reservas = await query
                    .OrderBy(reserva => reserva.FechaReserva)
                    .ThenBy(reserva => reserva.HoraInicio)
                    .ToListAsync(),
                Consorcios = await GetConsorciosAsync(),
                Estado = estado,
                ConsorcioId = consorcioId,
                Fecha = fecha?.Date,
                Busqueda = normalizedBusqueda
            };
        }

        public async Task<ReservaCreateViewModel> BuildReservaCreateViewModelAsync(string email, ReservaCreateViewModel? model = null)
        {
            var viewModel = model ?? new ReservaCreateViewModel();
            viewModel.UnidadesFuncionales = await GetOwnerUnidadesAsync(email);
            viewModel.Amenities = await GetOwnerAmenitiesAsync(viewModel.UnidadesFuncionales);

            if (viewModel.UnidadFuncionalId == 0 && viewModel.UnidadesFuncionales.Count == 1)
            {
                viewModel.UnidadFuncionalId = viewModel.UnidadesFuncionales[0].Id;
            }

            return viewModel;
        }

        public async Task<MisReservasViewModel> GetMisReservasAsync(string email, EstadoReserva? estado = null, string? busqueda = null)
        {
            var normalizedBusqueda = NormalizeOptional(busqueda);
            var unidades = await GetOwnerUnidadesAsync(email);
            var unidadIds = unidades.Select(unidad => unidad.Id).ToList();

            var query = _context.Reservas
                .Include(reserva => reserva.Amenity)
                    .ThenInclude(amenity => amenity.Consorcio)
                .Include(reserva => reserva.UnidadFuncional)
                .AsNoTracking()
                .Where(reserva => unidadIds.Contains(reserva.UnidadFuncionalId));

            if (estado.HasValue)
            {
                query = query.Where(reserva => reserva.Estado == estado.Value);
            }

            if (!string.IsNullOrWhiteSpace(normalizedBusqueda))
            {
                var pattern = $"%{normalizedBusqueda}%";
                query = query.Where(reserva =>
                    EF.Functions.Like(reserva.Amenity.Nombre, pattern)
                    || (reserva.Observaciones != null && EF.Functions.Like(reserva.Observaciones, pattern)));
            }

            return new MisReservasViewModel
            {
                Reservas = await query
                    .OrderByDescending(reserva => reserva.FechaReserva)
                    .ThenBy(reserva => reserva.HoraInicio)
                    .ToListAsync(),
                UnidadesFuncionales = unidades,
                Estado = estado,
                Busqueda = normalizedBusqueda
            };
        }

        public async Task<ReservaDetailsViewModel?> GetDetailsAsync(int id)
        {
            var reserva = await _context.Reservas
                .Include(item => item.Amenity)
                    .ThenInclude(amenity => amenity.Consorcio)
                .Include(item => item.UnidadFuncional)
                .AsNoTracking()
                .FirstOrDefaultAsync(item => item.Id == id);

            return reserva is null ? null : MapDetails(reserva);
        }

        public async Task<bool> OwnerCanAccessAsync(int reservaId, string email)
        {
            var normalizedEmail = Normalize(email);

            return await _context.Reservas.AnyAsync(reserva =>
                reserva.Id == reservaId
                && reserva.UnidadFuncional.MailPropietario == normalizedEmail);
        }

        public async Task<List<string>> ValidateReservaAsync(ReservaCreateViewModel model, string email)
        {
            var errors = new List<string>();
            var normalizedEmail = Normalize(email);
            var today = DateTime.Today;
            var selectedDate = model.FechaReserva.Date;

            if (string.IsNullOrWhiteSpace(normalizedEmail))
            {
                errors.Add("Debe iniciar sesion para crear una reserva.");
                return errors;
            }

            var unidad = await _context.UnidadesFuncionales
                .AsNoTracking()
                .FirstOrDefaultAsync(item => item.Id == model.UnidadFuncionalId);

            if (unidad is null)
            {
                errors.Add("Debe seleccionar una unidad funcional valida.");
            }
            else if (!string.Equals(unidad.MailPropietario, normalizedEmail, StringComparison.OrdinalIgnoreCase))
            {
                errors.Add("La unidad funcional seleccionada no pertenece al propietario autenticado.");
            }

            var amenity = await _context.Amenities
                .AsNoTracking()
                .FirstOrDefaultAsync(item => item.Id == model.AmenityId);

            if (amenity is null)
            {
                errors.Add("Debe seleccionar un amenity valido.");
            }
            else if (!amenity.Activo)
            {
                errors.Add("El amenity seleccionado no esta activo.");
            }

            if (unidad is not null && amenity is not null && amenity.ConsorcioId != unidad.ConsorcioId)
            {
                errors.Add("El amenity debe pertenecer al mismo consorcio que la unidad funcional.");
            }

            if (model.FechaReserva == default)
            {
                errors.Add("La fecha de reserva es obligatoria.");
            }
            else if (selectedDate < today)
            {
                errors.Add("La fecha de reserva no puede ser anterior a hoy.");
            }

            if (model.HoraInicio >= model.HoraFin)
            {
                errors.Add("La hora de inicio debe ser menor a la hora de fin.");
            }

            if (unidad is not null && await HasDeudaVencidaAsync(unidad.Id, today))
            {
                errors.Add("No se puede reservar porque la unidad funcional tiene deuda vencida o expensa vencida no pagada.");
            }

            if (unidad is not null && selectedDate != default)
            {
                var reservasDelMes = await _context.Reservas.CountAsync(reserva =>
                    reserva.UnidadFuncionalId == unidad.Id
                    && reserva.Estado != EstadoReserva.Cancelada
                    && reserva.FechaReserva.Year == selectedDate.Year
                    && reserva.FechaReserva.Month == selectedDate.Month);

                if (reservasDelMes >= MaxReservasMensualesPorUf)
                {
                    errors.Add("La unidad funcional ya alcanzo el maximo de 5 reservas en el mes.");
                }
            }

            if (amenity is not null && model.HoraInicio < model.HoraFin && selectedDate != default)
            {
                var hasOverlap = await _context.Reservas.AnyAsync(reserva =>
                    reserva.AmenityId == amenity.Id
                    && reserva.Estado != EstadoReserva.Cancelada
                    && reserva.FechaReserva.Date == selectedDate
                    && reserva.HoraInicio < model.HoraFin
                    && model.HoraInicio < reserva.HoraFin);

                if (hasOverlap)
                {
                    errors.Add("Ya existe una reserva para el mismo amenity en esa fecha y franja horaria.");
                }
            }

            return errors;
        }

        public async Task<Reserva> CreateReservaAsync(ReservaCreateViewModel model)
        {
            var reserva = new Reserva
            {
                AmenityId = model.AmenityId,
                UnidadFuncionalId = model.UnidadFuncionalId,
                FechaReserva = model.FechaReserva.Date,
                HoraInicio = model.HoraInicio,
                HoraFin = model.HoraFin,
                Estado = EstadoReserva.Confirmada,
                Observaciones = NormalizeOptional(model.Observaciones)
            };

            _context.Reservas.Add(reserva);
            await _context.SaveChangesAsync();

            return reserva;
        }

        public async Task<(bool Success, string? Error)> CancelarAsync(int reservaId, string email)
        {
            var normalizedEmail = Normalize(email);
            var reserva = await _context.Reservas
                .Include(item => item.UnidadFuncional)
                .FirstOrDefaultAsync(item => item.Id == reservaId);

            if (reserva is null)
            {
                return (false, "La reserva no existe.");
            }

            if (!string.Equals(reserva.UnidadFuncional.MailPropietario, normalizedEmail, StringComparison.OrdinalIgnoreCase))
            {
                return (false, "La reserva no pertenece al propietario autenticado.");
            }

            if (reserva.Estado == EstadoReserva.Cancelada)
            {
                return (false, "La reserva ya esta cancelada.");
            }

            if (reserva.FechaReserva.Date < DateTime.Today)
            {
                return (false, "No se puede cancelar una reserva pasada.");
            }

            reserva.Estado = EstadoReserva.Cancelada;
            await _context.SaveChangesAsync();

            return (true, null);
        }

        private async Task<bool> HasDeudaVencidaAsync(int unidadFuncionalId, DateTime today)
        {
            return await _context.Expensas.AnyAsync(expensa =>
                expensa.UnidadFuncionalId == unidadFuncionalId
                && expensa.Estado != EstadoExpensa.Pagada
                && (expensa.Estado == EstadoExpensa.Vencida || expensa.FechaVencimiento < today));
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

        private async Task<List<Amenity>> GetOwnerAmenitiesAsync(List<UnidadFuncional> unidades)
        {
            var consorcioIds = unidades
                .Select(unidad => unidad.ConsorcioId)
                .Distinct()
                .ToList();

            if (consorcioIds.Count == 0)
            {
                return [];
            }

            return await _context.Amenities
                .Include(amenity => amenity.Consorcio)
                .AsNoTracking()
                .Where(amenity => amenity.Activo && consorcioIds.Contains(amenity.ConsorcioId))
                .OrderBy(amenity => amenity.Consorcio.Nombre)
                .ThenBy(amenity => amenity.Nombre)
                .ToListAsync();
        }

        private static ReservaDetailsViewModel MapDetails(Reserva reserva)
        {
            var unidad = reserva.UnidadFuncional;
            var amenity = reserva.Amenity;

            return new ReservaDetailsViewModel
            {
                Id = reserva.Id,
                AmenityId = reserva.AmenityId,
                AmenityNombre = amenity.Nombre,
                AmenityDescripcion = amenity.Descripcion,
                AmenityCapacidad = amenity.Capacidad,
                UnidadFuncionalId = reserva.UnidadFuncionalId,
                UnidadFuncionalNumero = unidad.NumeroUF,
                Piso = unidad.Piso,
                Departamento = unidad.Departamento,
                PropietarioNombre = unidad.NombrePropietario,
                PropietarioMail = unidad.MailPropietario,
                ConsorcioId = amenity.ConsorcioId,
                ConsorcioNombre = amenity.Consorcio.Nombre,
                FechaReserva = reserva.FechaReserva,
                HoraInicio = reserva.HoraInicio,
                HoraFin = reserva.HoraFin,
                Estado = reserva.Estado,
                Observaciones = reserva.Observaciones
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
