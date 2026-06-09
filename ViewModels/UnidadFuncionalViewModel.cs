using System.ComponentModel.DataAnnotations;

namespace GestionDeConsorciosMVC.ViewModels
{
    public class UnidadFuncionalViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El numero de UF es obligatorio.")]
        [StringLength(50, ErrorMessage = "El numero de UF no puede superar los 50 caracteres.")]
        public string NumeroUF { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "El piso no puede superar los 50 caracteres.")]
        public string Piso { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "El departamento no puede superar los 50 caracteres.")]
        public string Departamento { get; set; } = string.Empty;

        [StringLength(150, ErrorMessage = "El nombre del propietario no puede superar los 150 caracteres.")]
        public string NombrePropietario { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "El email del propietario no tiene un formato valido.")]
        [StringLength(150, ErrorMessage = "El email del propietario no puede superar los 150 caracteres.")]
        public string? MailPropietario { get; set; }

        [Required(ErrorMessage = "El DNI del propietario es obligatorio.")]
        [StringLength(30, ErrorMessage = "El DNI del propietario no puede superar los 30 caracteres.")]
        public string DniPropietario { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "El telefono no puede superar los 50 caracteres.")]
        public string? Telefono { get; set; }

        public EstadoUnidadFuncional Estado { get; set; } = EstadoUnidadFuncional.Activa;
    }
}
