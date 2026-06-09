using System.ComponentModel.DataAnnotations;

namespace GestionDeConsorciosMVC.ViewModels
{
    public class RevisarPagoViewModel
    {
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un pago valido.")]
        public int PagoId { get; set; }

        [StringLength(1000, ErrorMessage = "La observacion no puede superar los 1000 caracteres.")]
        public string? ObservacionAdministracion { get; set; }

        public string? ReturnUrl { get; set; }
    }
}
