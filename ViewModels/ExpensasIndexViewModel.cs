namespace GestionDeConsorciosMVC.ViewModels
{
    public class ExpensasIndexViewModel
    {
        public List<Expensa> Expensas { get; set; } = new();
        public List<Consorcio> Consorcios { get; set; } = new();

        public int? ConsorcioId { get; set; }
        public string? Periodo { get; set; }
        public int? Mes { get; set; }
        public int? Anio { get; set; }
        public EstadoExpensa? Estado { get; set; }

        public int CantidadExpensas => Expensas.Count;
        public decimal TotalEmitido => Expensas.Sum(expensa => expensa.MontoTotal);
        public decimal TotalPagado => Expensas
            .SelectMany(expensa => expensa.Pagos)
            .Where(pago => pago.Estado == EstadoPago.Aprobado)
            .Sum(pago => pago.MontoPagado);
        public int Pendientes => Expensas.Count(expensa => expensa.Estado == EstadoExpensa.Pendiente);
    }
}
