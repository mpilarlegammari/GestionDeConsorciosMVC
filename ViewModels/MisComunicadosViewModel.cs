namespace GestionDeConsorciosMVC.ViewModels
{
    public class MisComunicadosViewModel
    {
        public List<Comunicado> Comunicados { get; set; } = new();
        public List<string> ConsorciosNombres { get; set; } = new();

        public bool? Importante { get; set; }
        public string? Busqueda { get; set; }

        public int CantidadComunicados => Comunicados.Count;
        public int CantidadImportantes => Comunicados.Count(comunicado => comunicado.Importante);
        public int CantidadConAdjunto => Comunicados.Count(comunicado => !string.IsNullOrWhiteSpace(comunicado.ArchivoAdjuntoPath));
        public string ConsorciosResumen => ConsorciosNombres.Count == 0
            ? "Sin consorcio asociado"
            : string.Join(", ", ConsorciosNombres);
    }
}
