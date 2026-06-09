using GestionDeConsorciosMVC.ViewModels;

namespace GestionDeConsorciosMVC.Services
{
    public interface IGastoService
    {
        Task<List<Gasto>> GetAllAsync(int? consorcioId = null);
        Task<Gasto?> GetByIdAsync(int id);
        Task<GastoVM?> GetForEditAsync(int id);
        Task<List<Consorcio>> GetConsorciosAsync();
        Task<bool> ExistsNumeroFacturaAsync(string numeroFactura, int? excludeGastoId = null);
        Task<Gasto> CreateAsync(GastoVM model, string archivoFacturaPath);
        Task<bool> UpdateAsync(GastoVM model, string? archivoFacturaPath = null);
        Task<bool> DeleteAsync(int id);
    }
}