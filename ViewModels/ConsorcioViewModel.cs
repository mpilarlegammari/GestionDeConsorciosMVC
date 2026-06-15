using System.ComponentModel.DataAnnotations;

namespace GestionDeConsorciosMVC.ViewModels
{
    public class ConsorcioViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del consorcio es obligatorio.")]
        [StringLength(150, ErrorMessage = "El nombre del consorcio no puede superar los 150 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El CUIT es obligatorio.")]
        [StringLength(30, ErrorMessage = "El CUIT no puede superar los 30 caracteres.")]
        public string Cuit { get; set; } = string.Empty;

        [Required(ErrorMessage = "La direccion es obligatoria.")]
        [StringLength(200, ErrorMessage = "La direccion no puede superar los 200 caracteres.")]
        public string Direccion { get; set; } = string.Empty;

        [Required(ErrorMessage = "La ciudad es obligatoria.")]
        [StringLength(100, ErrorMessage = "La ciudad no puede superar los 100 caracteres.")]
        public string Ciudad { get; set; } = string.Empty;

        [Required(ErrorMessage = "El codigo postal es obligatorio.")]
        [StringLength(20, ErrorMessage = "El codigo postal no puede superar los 20 caracteres.")]
        public string CodigoPostal { get; set; } = string.Empty;

        [Range(1, 500, ErrorMessage = "La cantidad de pisos debe ser mayor a 0.")]
        public int CantidadPisos { get; set; } = 1;

        [StringLength(1000, ErrorMessage = "Las observaciones no pueden superar los 1000 caracteres.")]
        public string? Observaciones { get; set; }

        public EstadoConsorcio Estado { get; set; } = EstadoConsorcio.Activo;

        public List<UnidadFuncionalViewModel> UnidadesFuncionales { get; set; } = new()
        {
            new UnidadFuncionalViewModel()
        };

        public List<AmenityViewModel> Amenities { get; set; } = new()
        {
            new AmenityViewModel()
        };
    }
}
