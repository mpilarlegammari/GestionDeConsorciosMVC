/// <summary>
/// Representa un gasto cargado por la administracion para un consorcio.
/// TODO backend: validar factura duplicada y archivo adjunto en servidor.
/// </summary>
public class Gasto
{
    public int Id { get; set; }
    public int ConsorcioId { get; set; }
    public string NumeroFactura { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public decimal Monto { get; set; }
    public string Concepto { get; set; } = string.Empty;
    public CategoriaGasto Categoria { get; set; } = CategoriaGasto.Otros;
    public string? Descripcion { get; set; }
    public string? ArchivoFacturaPath { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public Consorcio Consorcio { get; set; } = null!;
}
