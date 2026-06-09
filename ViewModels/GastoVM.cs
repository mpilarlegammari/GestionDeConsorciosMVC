using System.ComponentModel.DataAnnotations;

namespace GestionDeConsorciosMVC.ViewModels
{
    public class GastoVM
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un consorcio.")]
        public int ConsorcioId { get; set; }

        [Required(ErrorMessage = "Debe ingresar el numero de factura.")]
        [StringLength(100, ErrorMessage = "El numero de factura no puede superar los 100 caracteres.")]
        public string NumeroFactura { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe ingresar la fecha del gasto.")]
        public DateTime Fecha { get; set; } = DateTime.Today;

        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a cero.")]
        public decimal Monto { get; set; }

        [Required(ErrorMessage = "Debe ingresar el concepto del gasto.")]
        [StringLength(200, ErrorMessage = "El concepto no puede superar los 200 caracteres.")]
        public string Concepto { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe seleccionar una categoria.")]
        [StringLength(100, ErrorMessage = "La categoria no puede superar los 100 caracteres.")]
        public string Categoria { get; set; } = string.Empty;

        public string? Descripcion { get; set; }
        public string? ArchivoFacturaPath { get; set; }
    }
}
