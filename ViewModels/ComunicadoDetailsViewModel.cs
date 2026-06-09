namespace GestionDeConsorciosMVC.ViewModels
{
    public class ComunicadoDetailsViewModel
    {
        public int Id { get; set; }
        public int ConsorcioId { get; set; }
        public string ConsorcioNombre { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;
        public DateTime FechaPublicacion { get; set; }
        public string? ArchivoAdjuntoPath { get; set; }
        public bool Importante { get; set; }

        public string DetailRole { get; set; } = "Administrador";
        public string ReturnUrl { get; set; } = "/Comunicados";
    }
}
