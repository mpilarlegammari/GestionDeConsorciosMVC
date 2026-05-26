/// <summary>
/// Comunicado publicado por la administracion para un consorcio.
/// TODO backend: definir alcance, adjuntos y reglas de visibilidad por rol.
/// </summary>
public class Comunicado
{
    public int Id { get; set; }
    public int ConsorcioId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
    public DateTime FechaPublicacion { get; set; } = DateTime.UtcNow;
    public string? ArchivoAdjuntoPath { get; set; }
    public bool Importante { get; set; }

    public Consorcio Consorcio { get; set; } = null!;
}
