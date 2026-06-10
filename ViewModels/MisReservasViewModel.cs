namespace GestionDeConsorciosMVC.ViewModels
{
    public class MisReservasViewModel
    {
        public List<Reserva> Reservas { get; set; } = new();
        public List<UnidadFuncional> UnidadesFuncionales { get; set; } = new();

        public EstadoReserva? Estado { get; set; }
        public string? Busqueda { get; set; }

        public int CantidadReservas => Reservas.Count;
        public int CantidadConfirmadas => Reservas.Count(reserva => reserva.Estado == EstadoReserva.Confirmada);
        public int CantidadPendientes => Reservas.Count(reserva => reserva.Estado == EstadoReserva.Pendiente);
        public int CantidadCanceladas => Reservas.Count(reserva => reserva.Estado == EstadoReserva.Cancelada);
        public string UnidadesResumen => UnidadesFuncionales.Count == 0
            ? "Sin unidad asociada"
            : string.Join(", ", UnidadesFuncionales.Select(unidad => $"UF {unidad.NumeroUF}"));
    }
}
