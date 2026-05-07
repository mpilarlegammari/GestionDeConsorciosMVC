using System;

public class UnidadFuncional
{
    public int Id { get; set; }
    public string Propietario { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public int ConsorcioId { get; set; }
    public Consorcio Consorcio { get; set; }
}
