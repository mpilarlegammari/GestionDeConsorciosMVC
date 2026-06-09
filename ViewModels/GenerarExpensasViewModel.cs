using System.ComponentModel.DataAnnotations;

namespace GestionDeConsorciosMVC.ViewModels
{
    public class GenerarExpensasViewModel
    {
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un consorcio valido.")]
        public int ConsorcioId { get; set; }

        [Required(ErrorMessage = "El periodo es obligatorio.")]
        public string Periodo { get; set; } = DateTime.Today.ToString("yyyy-MM");

        [Required(ErrorMessage = "La fecha de emision es obligatoria.")]
        public DateTime FechaEmision { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "La fecha de vencimiento es obligatoria.")]
        public DateTime FechaVencimiento { get; set; } = DateTime.Today.AddDays(10);

        [Required(ErrorMessage = "Debe seleccionar un criterio de distribucion.")]
        public string CriterioDistribucion { get; set; } = "Partes iguales por UF";

        public string? Observaciones { get; set; }

        public List<Consorcio> Consorcios { get; set; } = new();
        public List<Gasto> Gastos { get; set; } = new();

        public int CantidadUnidades { get; set; }
        public decimal TotalGastos => Gastos.Sum(gasto => gasto.Monto);
        public decimal MontoPorUnidad => CantidadUnidades == 0 ? 0 : Math.Round(TotalGastos / CantidadUnidades, 2);
    }
}
