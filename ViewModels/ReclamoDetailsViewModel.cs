namespace GestionDeConsorciosMVC.ViewModels
{
    public class ReclamoDetailsViewModel
    {
        public int Id { get; set; }
        public int UnidadFuncionalId { get; set; }
        public string UnidadFuncionalNumero { get; set; } = string.Empty;
        public string Piso { get; set; } = string.Empty;
        public string Departamento { get; set; } = string.Empty;
        public string PropietarioNombre { get; set; } = string.Empty;
        public string PropietarioMail { get; set; } = string.Empty;
        public int ConsorcioId { get; set; }
        public string ConsorcioNombre { get; set; } = string.Empty;

        public string Asunto { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public EstadoReclamo Estado { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaCierre { get; set; }
        public string? ObservacionAdministracion { get; set; }

        public string DetailRole { get; set; } = "Administrador";
        public string ReturnUrl { get; set; } = "/Reclamos";
    }
}
