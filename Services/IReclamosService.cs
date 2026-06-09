using GestionDeConsorciosMVC.ViewModels;

namespace GestionDeConsorciosMVC.Services
{
    public interface IReclamosService
    {
        Task<ReclamosAdminIndexViewModel> GetAdminIndexAsync(
            EstadoReclamo? estado = null,
            int? consorcioId = null,
            string? busqueda = null);

        Task<ReclamoCreateViewModel> BuildCreateViewModelAsync(string email, ReclamoCreateViewModel? model = null);
        Task<MisReclamosViewModel> GetMisReclamosAsync(string email, EstadoReclamo? estado = null, string? busqueda = null);
        Task<ReclamoDetailsViewModel?> GetDetailsAsync(int id);
        Task<bool> OwnerCanAccessAsync(int reclamoId, string email);
        Task<bool> OwnerHasUnidadesAsync(string email);
        Task<bool> UnidadFuncionalBelongsToOwnerAsync(int unidadFuncionalId, string email);
        Task<Reclamo> CreateAsync(ReclamoCreateViewModel model);
        Task<bool> CambiarEstadoAsync(CambiarEstadoReclamoViewModel model);
    }
}
