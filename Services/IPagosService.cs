using GestionDeConsorciosMVC.ViewModels;

namespace GestionDeConsorciosMVC.Services
{
    public interface IPagosService
    {
        Task<PagosAdminIndexViewModel> GetAdminIndexAsync(
            EstadoPago? estado = null,
            int? consorcioId = null,
            string? periodo = null);

        Task<PagoDetailsViewModel?> GetDetailsAsync(int id);
        Task<bool> AprobarAsync(RevisarPagoViewModel model);
        Task<bool> RechazarAsync(RevisarPagoViewModel model);
    }
}
