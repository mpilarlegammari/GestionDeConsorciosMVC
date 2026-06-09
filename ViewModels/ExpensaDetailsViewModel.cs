namespace GestionDeConsorciosMVC.ViewModels
{
    public class ExpensaDetailsViewModel
    {
        public int Id { get; set; }
        public string Periodo { get; set; } = string.Empty;
        public DateTime FechaEmision { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public decimal MontoTotal { get; set; }
        public EstadoExpensa Estado { get; set; }
        public string? Observaciones { get; set; }

        public int ConsorcioId { get; set; }
        public string ConsorcioNombre { get; set; } = string.Empty;
        public int UnidadFuncionalId { get; set; }
        public string UnidadFuncionalNumero { get; set; } = string.Empty;
        public string PropietarioNombre { get; set; } = string.Empty;
        public string PropietarioMail { get; set; } = string.Empty;

        public List<Pago> Pagos { get; set; } = new();
        public List<Gasto> Gastos { get; set; } = new();

        public string ReturnUrl { get; set; } = "/Expensas";
        public string DetailRole { get; set; } = "Administrador";

        public decimal TotalPagadoAprobado => Pagos
            .Where(pago => pago.Estado == EstadoPago.Aprobado)
            .Sum(pago => pago.MontoPagado);
        public int PagosPendientes => Pagos.Count(pago => pago.Estado == EstadoPago.PendienteRevision);
        public decimal TotalGeneralGastos => Gastos.Sum(gasto => gasto.Monto);
    }
}
