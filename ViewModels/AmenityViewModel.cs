using System.ComponentModel.DataAnnotations;

namespace GestionDeConsorciosMVC.ViewModels
{
    public class AmenityViewModel
    {
        public int Id { get; set; }

        [StringLength(120, ErrorMessage = "El nombre del amenity no puede superar los 120 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "La descripcion no puede superar los 1000 caracteres.")]
        public string? Descripcion { get; set; }

        [Range(1, 1000, ErrorMessage = "La capacidad debe ser mayor a 0.")]
        public int Capacidad { get; set; } = 1;

        public bool Activo { get; set; } = true;
    }
}
