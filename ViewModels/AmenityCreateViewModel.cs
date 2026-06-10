using System.ComponentModel.DataAnnotations;

namespace GestionDeConsorciosMVC.ViewModels
{
    public class AmenityCreateViewModel
    {
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un consorcio valido.")]
        public int ConsorcioId { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(120, ErrorMessage = "El nombre no puede superar los 120 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "La descripcion no puede superar los 1000 caracteres.")]
        public string? Descripcion { get; set; }

        [Range(1, 1000, ErrorMessage = "La capacidad debe ser mayor a 0.")]
        public int Capacidad { get; set; } = 1;

        public bool Activo { get; set; } = true;
        public List<Consorcio> Consorcios { get; set; } = new();
    }
}
