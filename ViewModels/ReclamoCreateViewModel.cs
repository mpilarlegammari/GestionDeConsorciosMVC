using System.ComponentModel.DataAnnotations;

namespace GestionDeConsorciosMVC.ViewModels
{
    public class ReclamoCreateViewModel
    {
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una unidad funcional valida.")]
        public int UnidadFuncionalId { get; set; }

        [Required(ErrorMessage = "El asunto es obligatorio.")]
        [StringLength(150, ErrorMessage = "El asunto no puede superar los 150 caracteres.")]
        public string Asunto { get; set; } = string.Empty;

        [Required(ErrorMessage = "La categoria es obligatoria.")]
        [StringLength(80, ErrorMessage = "La categoria no puede superar los 80 caracteres.")]
        public string Categoria { get; set; } = string.Empty;

        [Required(ErrorMessage = "La descripcion es obligatoria.")]
        [StringLength(2000, ErrorMessage = "La descripcion no puede superar los 2000 caracteres.")]
        public string Descripcion { get; set; } = string.Empty;

        public List<UnidadFuncional> UnidadesFuncionales { get; set; } = new();
        public List<string> Categorias { get; set; } = new();
    }
}
