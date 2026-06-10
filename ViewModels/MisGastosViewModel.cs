namespace GestionDeConsorciosMVC.ViewModels
{
    public class MisGastosViewModel
    {
        public List<Gasto> Gastos { get; set; } = new();
        public List<UnidadFuncional> UnidadesFuncionales { get; set; } = new();
        public List<UnidadFuncional> UnidadesSeleccionadas { get; set; } = new();
        public List<string> Categorias { get; set; } = new();
        public List<int> AniosDisponibles { get; set; } = new();

        public int? UnidadFuncionalId { get; set; }
        public int? Mes { get; set; }
        public int? Anio { get; set; }
        public string? Categoria { get; set; }
        public string? Busqueda { get; set; }

        public int CantidadGastos => Gastos.Count;
        public decimal TotalGastos => Gastos.Sum(gasto => gasto.Monto);

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

        public string PeriodoResumen
        {
            get
            {
                if (Mes.HasValue && Anio.HasValue)
                {
                    return $"{Mes.Value:00}/{Anio.Value}";
                }

                if (Mes.HasValue)
                {
                    return $"Mes {Mes.Value:00}";
                }

                if (Anio.HasValue)
                {
                    return Anio.Value.ToString();
                }

                return "Todos los periodos";
            }
        }

        public IReadOnlyDictionary<string, decimal> ResumenPorCategoria => Gastos
            .GroupBy(gasto => gasto.Categoria)
            .OrderBy(group => group.Key)
            .ToDictionary(group => group.Key, group => group.Sum(gasto => gasto.Monto));

        private List<UnidadFuncional> UnidadesParaResumen => UnidadesSeleccionadas.Count == 0
            ? UnidadesFuncionales
            : UnidadesSeleccionadas;
    }
}
