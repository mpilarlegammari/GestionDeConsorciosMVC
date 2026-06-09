using GestionDeConsorciosMVC.ViewModels;

namespace GestionDeConsorciosMVC.Services
{
    public interface IExpensasService
    {
        Task<ExpensasIndexViewModel> GetIndexAsync(
            int? consorcioId = null,
            string? periodo = null,
            int? mes = null,
            int? anio = null,
            EstadoExpensa? estado = null);

        Task<GenerarExpensasViewModel> BuildGenerarViewModelAsync(GenerarExpensasViewModel? model = null);
        Task<ExpensaDetailsViewModel?> GetDetailsAsync(int id);
        Task<Consorcio?> GetConsorcioConUnidadesAsync(int consorcioId);
        Task<bool> ExistsPeriodoGeneradoAsync(int consorcioId, string periodo);
        Task<List<Gasto>> GetGastosPeriodoAsync(int consorcioId, string periodo);
        Task<int> GenerarAsync(GenerarExpensasViewModel model);
    }
}
