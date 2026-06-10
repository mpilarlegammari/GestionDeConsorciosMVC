namespace GestionDeConsorciosMVC.ViewModels
{
    public class MisPagosViewModel
    {
        public List<Pago> Pagos { get; set; } = new();
        public List<UnidadFuncional> UnidadesFuncionales { get; set; } = new();
        public List<UnidadFuncional> UnidadesSeleccionadas { get; set; } = new();
        public List<string> PeriodosDisponibles { get; set; } = new();
        public List<int> AniosDisponibles { get; set; } = new();
        public List<EstadoPago> EstadosDisponibles { get; set; } = Enum.GetValues<EstadoPago>().ToList();
        public List<string> MediosPagoDisponibles { get; set; } = new();

        public int? UnidadFuncionalId { get; set; }
        public string? Periodo { get; set; }
        public int? Anio { get; set; }
        public EstadoPago? Estado { get; set; }
        public string? MedioPago { get; set; }

        public int CantidadPagos => Pagos.Count;
        public int CantidadPendientes => Pagos.Count(pago => pago.Estado == EstadoPago.PendienteRevision);
        public int CantidadAprobados => Pagos.Count(pago => pago.Estado == EstadoPago.Aprobado);
        public int CantidadRechazados => Pagos.Count(pago => pago.Estado == EstadoPago.Rechazado);
        public decimal TotalPagos => Pagos.Sum(pago => pago.MontoPagado);
        public decimal TotalPendienteRevision => Pagos
            .Where(pago => pago.Estado == EstadoPago.PendienteRevision)
            .Sum(pago => pago.MontoPagado);
        public decimal TotalAprobado => Pagos
            .Where(pago => pago.Estado == EstadoPago.Aprobado)
            .Sum(pago => pago.MontoPagado);
        public decimal TotalRechazado => Pagos
            .Where(pago => pago.Estado == EstadoPago.Rechazado)
            .Sum(pago => pago.MontoPagado);

        public string UnidadesResumen => UnidadesParaResumen.Count == 0
            ? "Sin unidad asociada"
            : string.Join(", ", UnidadesParaResumen.Select(unidad => $"UF {unidad.NumeroUF}"));

        public string ConsorciosResumen
        {
            get
            {
                var nombres = UnidadesParaResumen
                    .Select(unidad => unidad.Consorcio.Nombre)
                    .Distinct()
                    .ToList();

                return nombres.Count == 0 ? "Sin consorcio asociado" : string.Join(", ", nombres);
            }
        }

        private List<UnidadFuncional> UnidadesParaResumen => UnidadesSeleccionadas.Count == 0
            ? UnidadesFuncionales
            : UnidadesSeleccionadas;
    }
}
