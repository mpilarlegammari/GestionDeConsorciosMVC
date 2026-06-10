namespace GestionDeConsorciosMVC.ViewModels
{
    public class ReservasAdminIndexViewModel
    {
        public List<Reserva> Reservas { get; set; } = new();
        public List<Consorcio> Consorcios { get; set; } = new();

        public EstadoReserva? Estado { get; set; }
        public int? ConsorcioId { get; set; }
        public DateTime? Fecha { get; set; }
        public string? Busqueda { get; set; }

        public int CantidadReservas => Reservas.Count;
        public int CantidadPendientes => Reservas.Count(reserva => reserva.Estado == EstadoReserva.Pendiente);
        public int CantidadConfirmadas => Reservas.Count(reserva => reserva.Estado == EstadoReserva.Confirmada);
        public int CantidadCanceladas => Reservas.Count(reserva => reserva.Estado == EstadoReserva.Cancelada);
    }
}
