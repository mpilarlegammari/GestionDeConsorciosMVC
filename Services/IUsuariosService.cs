namespace GestionDeConsorciosMVC.Services
{
    public interface IUsuariosService
    {
        Task EnsurePropietarioUsersAsync(IEnumerable<UnidadFuncional> unidadesFuncionales);
    }
}
