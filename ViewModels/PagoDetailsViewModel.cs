namespace GestionDeConsorciosMVC.ViewModels
{
    public class PagoDetailsViewModel
    {
        public int Id { get; set; }
        public int ExpensaId { get; set; }
        public string Periodo { get; set; } = string.Empty;
        public decimal MontoExpensa { get; set; }
        public EstadoExpensa EstadoExpensa { get; set; }

        public string ConsorcioNombre { get; set; } = string.Empty;
        public string UnidadFuncionalNumero { get; set; } = string.Empty;
        public string PropietarioNombre { get; set; } = string.Empty;
        public string PropietarioMail { get; set; } = string.Empty;

        public DateTime FechaPago { get; set; }
        public decimal MontoPagado { get; set; }
        public string MedioPago { get; set; } = string.Empty;
        public string? NumeroOperacion { get; set; }
        public string? BancoEntidad { get; set; }
        public string ComprobantePath { get; set; } = string.Empty;
        public string? Comentarios { get; set; }
        public EstadoPago Estado { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaRevision { get; set; }
        public string? ObservacionAdministracion { get; set; }

        public string DetailRole { get; set; } = "Propietario";
        public string ReturnUrl { get; set; } = "/Pagos/MisPagos";
    }
}
