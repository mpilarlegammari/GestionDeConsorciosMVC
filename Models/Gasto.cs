public class Gasto
{
    public int Id { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public string NumeroFactura { get; set; } = string.Empty;
    public DateOnly Fecha { get; set; }
    public decimal Monto { get; set; }
    public Categoria Categoria { get; set; }

    public int ConsorcioId { get; set; }
    public Consorcio Consorcio { get; set; }
}