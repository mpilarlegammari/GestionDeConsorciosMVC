using GestionDeConsorciosMVC.ViewModels;

namespace GestionDeConsorciosMVC.Services
{
    public interface IGastoService
    {
        Task<List<Gasto>> GetAllAsync(
            int? consorcioId = null,
            int? mes = null,
            int? anio = null,
            string? categoria = null,
            string? busqueda = null);
        Task<MisGastosViewModel> GetMisGastosAsync(
            string email,
            int? unidadFuncionalId = null,
            int? mes = null,
            int? anio = null,
            string? categoria = null,
            string? busqueda = null);
        Task<Gasto?> GetByIdAsync(int id);
        Task<GastoVM?> GetForEditAsync(int id);
        Task<List<Consorcio>> GetConsorciosAsync();
        Task<bool> ExistsNumeroFacturaAsync(string numeroFactura, int? excludeGastoId = null);
        Task<Gasto> CreateAsync(GastoVM model, string archivoFacturaPath);
        Task<bool> UpdateAsync(GastoVM model, string? archivoFacturaPath = null);
        Task<bool> DeleteAsync(int id);
    }
}
