namespace GestionDeConsorciosMVC.ViewModels
{
    public class MisExpensasViewModel
    {
        public List<Expensa> Expensas { get; set; } = new();
        public List<UnidadFuncional> UnidadesFuncionales { get; set; } = new();
        public List<UnidadFuncional> UnidadesSeleccionadas { get; set; } = new();
        public List<string> PeriodosDisponibles { get; set; } = new();
        public List<int> AniosDisponibles { get; set; } = new();

        public int? UnidadFuncionalId { get; set; }
        public string? Periodo { get; set; }
        public int? Anio { get; set; }
        public EstadoExpensa? Estado { get; set; }

        public Expensa? ExpensaActual => Expensas
            .OrderByDescending(expensa => expensa.FechaEmision)
            .FirstOrDefault();

        public int CantidadExpensas => Expensas.Count;
        public int CantidadPagadas => Expensas.Count(expensa => expensa.Estado == EstadoExpensa.Pagada);
        public int CantidadPendientes => Expensas.Count(expensa => expensa.Estado == EstadoExpensa.Pendiente);
        public int CantidadVencidas => Expensas.Count(expensa => expensa.Estado == EstadoExpensa.Vencida);
        public decimal TotalEmitido => Expensas.Sum(expensa => expensa.MontoTotal);
        public decimal TotalPendiente => Expensas
            .Where(expensa => expensa.Estado != EstadoExpensa.Pagada)
            .Sum(expensa => expensa.MontoTotal);
        public decimal TotalPagado => Expensas
            .Where(expensa => expensa.Estado == EstadoExpensa.Pagada)
            .Sum(expensa => expensa.MontoTotal);

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
