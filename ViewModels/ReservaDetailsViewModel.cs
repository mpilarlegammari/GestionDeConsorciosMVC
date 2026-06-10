namespace GestionDeConsorciosMVC.ViewModels
{
    public class ReservaDetailsViewModel
    {
        public int Id { get; set; }
        public int AmenityId { get; set; }
        public string AmenityNombre { get; set; } = string.Empty;
        public string? AmenityDescripcion { get; set; }
        public int AmenityCapacidad { get; set; }

        public int UnidadFuncionalId { get; set; }
        public string UnidadFuncionalNumero { get; set; } = string.Empty;
        public string Piso { get; set; } = string.Empty;
        public string Departamento { get; set; } = string.Empty;
        public string PropietarioNombre { get; set; } = string.Empty;
        public string PropietarioMail { get; set; } = string.Empty;

        public int ConsorcioId { get; set; }
        public string ConsorcioNombre { get; set; } = string.Empty;
        public DateTime FechaReserva { get; set; }
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFin { get; set; }
        public EstadoReserva Estado { get; set; }
        public string? Observaciones { get; set; }

        public string DetailRole { get; set; } = "Administrador";
        public string ReturnUrl { get; set; } = "/Reservas";
    }
}
