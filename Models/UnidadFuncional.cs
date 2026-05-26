/// <summary>
/// Representa una unidad funcional dentro de un consorcio.
/// TODO backend: vincular propietario real cuando exista autenticacion y usuarios persistidos.
/// </summary>
public class UnidadFuncional
{
    public int Id { get; set; }
    public string NumeroUF { get; set; } = string.Empty;
    public string Piso { get; set; } = string.Empty;
    public string Departamento { get; set; } = string.Empty;
    public string NombrePropietario { get; set; } = string.Empty;
    public string MailPropietario { get; set; } = string.Empty;
    public string DniPropietario { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public EstadoUnidadFuncional Estado { get; set; } = EstadoUnidadFuncional.Activa;
    public int ConsorcioId { get; set; }

    public Consorcio Consorcio { get; set; } = null!;
    public List<Expensa> Expensas { get; set; } = new();
    public List<Reserva> Reservas { get; set; } = new();
    public List<Reclamo> Reclamos { get; set; } = new();
}

public enum EstadoUnidadFuncional
{
    Activa,
    Morosa,
    Inactiva
}
