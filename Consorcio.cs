using System;

    public class Consorcio
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;

        public List<UnidadFuncional> UnidadesFuncionales { get; set; } = new();
        public List<Gasto> Gastos { get; set; } = new();
    }
