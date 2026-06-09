namespace GestionDeConsorciosMVC.ViewModels
{
    public class ConsorcioDetailsViewModel
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Cuit { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public string Ciudad { get; set; } = string.Empty;
        public string CodigoPostal { get; set; } = string.Empty;
        public int CantidadPisos { get; set; }
        public string? Observaciones { get; set; }
        public EstadoConsorcio Estado { get; set; }
        public DateTime FechaCreacion { get; set; }
        public int CantidadUnidades { get; set; }
        public int ExpensasPendientes { get; set; }
        public decimal TotalGastos { get; set; }
        public int CantidadGastos { get; set; }
        public int ReclamosAbiertos { get; set; }
        public List<UnidadFuncionalViewModel> UnidadesFuncionales { get; set; } = new();
    }
}
