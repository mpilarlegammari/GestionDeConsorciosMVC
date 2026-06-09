namespace GestionDeConsorciosMVC.ViewModels
{
    public class GastosIndexViewModel
    {
        public List<Gasto> Gastos { get; set; } = new();
        public List<Consorcio> Consorcios { get; set; } = new();
        public List<string> Categorias { get; set; } = new();

        public int? ConsorcioId { get; set; }
        public int? Mes { get; set; }
        public int? Anio { get; set; }
        public string? Categoria { get; set; }
        public string? Busqueda { get; set; }

        public int CantidadGastos => Gastos.Count;
        public decimal TotalGastos => Gastos.Sum(gasto => gasto.Monto);
        public decimal PromedioGastos => CantidadGastos == 0 ? 0 : TotalGastos / CantidadGastos;
    }
}
