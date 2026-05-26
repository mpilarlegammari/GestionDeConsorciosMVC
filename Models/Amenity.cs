/// <summary>
/// Espacio comun reservable dentro de un consorcio.
/// TODO backend: definir disponibilidad, horarios y reglas de reserva.
/// </summary>
public class Amenity
{
    public int Id { get; set; }
    public int ConsorcioId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public int Capacidad { get; set; }
    public bool Activo { get; set; } = true;

    public Consorcio Consorcio { get; set; } = null!;
    public List<Reserva> Reservas { get; set; } = new();
}
