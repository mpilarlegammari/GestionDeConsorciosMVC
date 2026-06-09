using System.ComponentModel.DataAnnotations;

namespace GestionDeConsorciosMVC.ViewModels
{
    public class CambiarEstadoReclamoViewModel
    {
        [Range(1, int.MaxValue, ErrorMessage = "El reclamo es obligatorio.")]
        public int ReclamoId { get; set; }

        [Required(ErrorMessage = "El estado es obligatorio.")]
        public EstadoReclamo Estado { get; set; }

        [StringLength(1000, ErrorMessage = "La observacion no puede superar los 1000 caracteres.")]
        public string? ObservacionAdministracion { get; set; }

        public string? ReturnUrl { get; set; }
    }
}
