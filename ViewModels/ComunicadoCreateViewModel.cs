using System.ComponentModel.DataAnnotations;

namespace GestionDeConsorciosMVC.ViewModels
{
    public class ComunicadoCreateViewModel
    {
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un consorcio valido.")]
        public int ConsorcioId { get; set; }

        [Required(ErrorMessage = "El titulo es obligatorio.")]
        [StringLength(200, ErrorMessage = "El titulo no puede superar los 200 caracteres.")]
        public string Titulo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El mensaje es obligatorio.")]
        [StringLength(4000, ErrorMessage = "El mensaje no puede superar los 4000 caracteres.")]
        public string Mensaje { get; set; } = string.Empty;

        public bool Importante { get; set; }
        public List<Consorcio> Consorcios { get; set; } = new();
    }
}
