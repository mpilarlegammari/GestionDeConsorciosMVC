using GestionDeConsorciosMVC.ViewModels;

namespace GestionDeConsorciosMVC.Services
{
    public interface IComunicadosService
    {
        Task<ComunicadosAdminIndexViewModel> GetAdminIndexAsync(
            int? consorcioId = null,
            bool? importante = null,
            string? busqueda = null);

        Task<ComunicadoCreateViewModel> BuildCreateViewModelAsync(ComunicadoCreateViewModel? model = null);
        Task<ComunicadoDetailsViewModel?> GetDetailsAsync(int id);
        Task<MisComunicadosViewModel> GetMisComunicadosAsync(string email, bool? importante = null, string? busqueda = null);
        Task<bool> ConsorcioExistsAsync(int consorcioId);
        Task<Comunicado> CreateAsync(ComunicadoCreateViewModel model, string? archivoAdjuntoPath);
    }
}
