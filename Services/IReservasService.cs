using GestionDeConsorciosMVC.ViewModels;

namespace GestionDeConsorciosMVC.Services
{
    public interface IReservasService
    {
        Task<AmenitiesIndexViewModel> GetAmenitiesIndexAsync(int? consorcioId = null, bool? activo = null, string? busqueda = null);
        Task<AmenityCreateViewModel> BuildAmenityCreateViewModelAsync(AmenityCreateViewModel? model = null);
        Task<bool> ConsorcioExistsAsync(int consorcioId);
        Task<Amenity> CreateAmenityAsync(AmenityCreateViewModel model);

        Task<ReservasAdminIndexViewModel> GetAdminIndexAsync(
            EstadoReserva? estado = null,
            int? consorcioId = null,
            DateTime? fecha = null,
            string? busqueda = null);

        Task<ReservaCreateViewModel> BuildReservaCreateViewModelAsync(string email, ReservaCreateViewModel? model = null);
        Task<MisReservasViewModel> GetMisReservasAsync(string email, EstadoReserva? estado = null, string? busqueda = null);
        Task<ReservaDetailsViewModel?> GetDetailsAsync(int id);
        Task<bool> OwnerCanAccessAsync(int reservaId, string email);
        Task<List<string>> ValidateReservaAsync(ReservaCreateViewModel model, string email);
        Task<Reserva> CreateReservaAsync(ReservaCreateViewModel model);
        Task<(bool Success, string? Error)> CancelarAsync(int reservaId, string email);
    }
}
