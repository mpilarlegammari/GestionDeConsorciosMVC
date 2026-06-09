namespace GestionDeConsorciosMVC.ViewModels
{
    public class ReclamosAdminIndexViewModel
    {
        public List<Reclamo> Reclamos { get; set; } = new();
        public List<Consorcio> Consorcios { get; set; } = new();

        public EstadoReclamo? Estado { get; set; }
        public int? ConsorcioId { get; set; }
        public string? Busqueda { get; set; }

        public int CantidadReclamos => Reclamos.Count;
        public int CantidadAbiertos => Reclamos.Count(reclamo => reclamo.Estado == EstadoReclamo.Abierto);
        public int CantidadEnProceso => Reclamos.Count(reclamo => reclamo.Estado == EstadoReclamo.EnProceso);
        public int CantidadCerrados => Reclamos.Count(reclamo => reclamo.Estado == EstadoReclamo.Cerrado);
    }
}
