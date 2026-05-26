/// <summary>
/// Liquidacion emitida para una unidad funcional en un periodo determinado.
/// TODO backend: calcular monto total y estado segun reglas de negocio.
/// </summary>
public class Expensa
{
    public int Id { get; set; }
    public int UnidadFuncionalId { get; set; }
    public string Periodo { get; set; } = string.Empty;
    public DateTime FechaEmision { get; set; }
    public DateTime FechaVencimiento { get; set; }
    public decimal MontoTotal { get; set; }
    public EstadoExpensa Estado { get; set; } = EstadoExpensa.Pendiente;
    public string? Observaciones { get; set; }

    public UnidadFuncional UnidadFuncional { get; set; } = null!;
    public List<Pago> Pagos { get; set; } = new();
}

public enum EstadoExpensa
{
    Pendiente,
    Pagada,
    Vencida
}
