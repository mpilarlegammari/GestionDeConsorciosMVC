/// <summary>
/// Representa un consorcio o edificio administrado por el sistema.
/// TODO backend: completar reglas de alta, edicion y baja logica.
/// </summary>
public class Consorcio
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Cuit { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
    public string Ciudad { get; set; } = string.Empty;
    public string CodigoPostal { get; set; } = string.Empty;
    public int CantidadPisos { get; set; }
    public string? Observaciones { get; set; }
    public EstadoConsorcio Estado { get; set; } = EstadoConsorcio.Pendiente;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public List<UnidadFuncional> UnidadesFuncionales { get; set; } = new();
    public List<Gasto> Gastos { get; set; } = new();
    public List<Comunicado> Comunicados { get; set; } = new();
    public List<Amenity> Amenities { get; set; } = new();
}

public enum EstadoConsorcio
{
    Activo,
    Pendiente,
    Inactivo
}
