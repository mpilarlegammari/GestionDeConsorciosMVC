namespace GestionDeConsorciosMVC.ViewModels
{
    public class MisReclamosViewModel
    {
        public List<Reclamo> Reclamos { get; set; } = new();
        public List<UnidadFuncional> UnidadesFuncionales { get; set; } = new();

        public EstadoReclamo? Estado { get; set; }
        public string? Busqueda { get; set; }

        public int CantidadReclamos => Reclamos.Count;
        public int CantidadAbiertos => Reclamos.Count(reclamo => reclamo.Estado == EstadoReclamo.Abierto);
        public int CantidadEnProceso => Reclamos.Count(reclamo => reclamo.Estado == EstadoReclamo.EnProceso);
        public int CantidadCerrados => Reclamos.Count(reclamo => reclamo.Estado == EstadoReclamo.Cerrado);
        public string UnidadesResumen => UnidadesFuncionales.Count == 0
            ? "Sin unidad asociada"
            : string.Join(", ", UnidadesFuncionales.Select(unidad => $"UF {unidad.NumeroUF}"));
    }
}
