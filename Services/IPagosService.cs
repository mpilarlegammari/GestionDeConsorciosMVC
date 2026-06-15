using GestionDeConsorciosMVC.ViewModels;

namespace GestionDeConsorciosMVC.Services
{
    public interface IPagosService
    {
        Task<PagosAdminIndexViewModel> GetAdminIndexAsync(
            EstadoPago? estado = null,
            int? consorcioId = null,
            string? periodo = null);

        Task<MisPagosViewModel> GetMisPagosAsync(
            string email,
            int? unidadFuncionalId = null,
            string? periodo = null,
            int? anio = null,
            EstadoPago? estado = null,
            string? medioPago = null);

        Task<PagoDetailsViewModel?> GetDetailsAsync(int id);
    }
}
