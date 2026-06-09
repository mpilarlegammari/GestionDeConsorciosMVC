namespace GestionDeConsorciosMVC.ViewModels
{
    public class PagosAdminIndexViewModel
    {
        public List<Pago> Pagos { get; set; } = new();
        public List<Consorcio> Consorcios { get; set; } = new();

        public EstadoPago? Estado { get; set; }
        public int? ConsorcioId { get; set; }
        public string? Periodo { get; set; }

        public int CantidadPagos => Pagos.Count;
        public int Pendientes => Pagos.Count(pago => pago.Estado == EstadoPago.PendienteRevision);
        public int Aprobados => Pagos.Count(pago => pago.Estado == EstadoPago.Aprobado);
        public int Rechazados => Pagos.Count(pago => pago.Estado == EstadoPago.Rechazado);
        public decimal TotalInformado => Pagos.Sum(pago => pago.MontoPagado);
    }
}
