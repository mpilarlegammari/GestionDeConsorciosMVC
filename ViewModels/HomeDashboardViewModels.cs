namespace GestionDeConsorciosMVC.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalConsorcios { get; set; }
        public int TotalUnidadesFuncionales { get; set; }
        public int ExpensasPendientes { get; set; }
        public decimal MontoExpensasPendientes { get; set; }
        public int PagosPendientesRevision { get; set; }
        public int ReclamosAbiertos { get; set; }
        public int ComunicadosImportantes { get; set; }
        public int ReservasPendientes { get; set; }
        public List<DashboardActivityItem> ActividadReciente { get; set; } = new();
        public List<DashboardConsorcioItem> ConsorciosRecientes { get; set; } = new();
    }

    public class PropietarioDashboardViewModel
    {
        public string Email { get; set; } = string.Empty;
        public List<UnidadFuncional> UnidadesFuncionales { get; set; } = new();
        public Expensa? ExpensaActual { get; set; }
        public int ExpensasPendientes { get; set; }
        public decimal MontoPendiente { get; set; }
        public int PagosPendientesRevision { get; set; }
        public int ReclamosAbiertos { get; set; }
        public int ReservasProximas { get; set; }
        public List<Comunicado> ComunicadosRecientes { get; set; } = new();
        public List<Pago> PagosRecientes { get; set; } = new();

        public string ConsorciosResumen => UnidadesFuncionales.Count == 0
            ? "Sin consorcio asociado"
            : string.Join(", ", UnidadesFuncionales.Select(unidad => unidad.Consorcio.Nombre).Distinct());

        public string UnidadesResumen => UnidadesFuncionales.Count == 0
            ? "Sin unidad asociada"
            : string.Join(", ", UnidadesFuncionales.Select(unidad => $"UF {unidad.NumeroUF}"));
    }

    public class DashboardActivityItem
    {
        public string Tipo { get; set; } = string.Empty;
        public string Detalle { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public string Tone { get; set; } = string.Empty;
    }

    public class DashboardConsorcioItem
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public int CantidadUnidades { get; set; }
        public EstadoConsorcio Estado { get; set; }
    }
}
