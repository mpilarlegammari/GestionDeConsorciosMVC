/// <summary>
/// Pago informado por un propietario sobre una expensa.
/// </summary>
public class Pago
{
    public int Id { get; set; }
    public int ExpensaId { get; set; }
    public DateTime FechaPago { get; set; }
    public decimal MontoPagado { get; set; }
    public string MedioPago { get; set; } = string.Empty;
    public string? NumeroOperacion { get; set; }
    public string? BancoEntidad { get; set; }
    public string ComprobantePath { get; set; } = string.Empty;
    public string? Comentarios { get; set; }
    public EstadoPago Estado { get; set; } = EstadoPago.Aprobado;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime? FechaRevision { get; set; }
    public string? ObservacionAdministracion { get; set; }

    public Expensa Expensa { get; set; } = null!;
}

public enum EstadoPago
{
    PendienteRevision,
    Aprobado,
    Rechazado
}
