namespace GestionDeConsorciosMVC.ViewModels
{
    public class ComunicadosAdminIndexViewModel
    {
        public List<Comunicado> Comunicados { get; set; } = new();
        public List<Consorcio> Consorcios { get; set; } = new();

        public int? ConsorcioId { get; set; }
        public bool? Importante { get; set; }
        public string? Busqueda { get; set; }

        public int CantidadComunicados => Comunicados.Count;
        public int CantidadImportantes => Comunicados.Count(comunicado => comunicado.Importante);
        public int CantidadConAdjunto => Comunicados.Count(comunicado => !string.IsNullOrWhiteSpace(comunicado.ArchivoAdjuntoPath));
    }
}
