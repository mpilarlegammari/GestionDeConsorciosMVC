using System.ComponentModel.DataAnnotations;

namespace GestionDeConsorciosMVC.ViewModels
{
    public class ReservaCreateViewModel
    {
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un amenity valido.")]
        public int AmenityId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una unidad funcional valida.")]
        public int UnidadFuncionalId { get; set; }

        [Required(ErrorMessage = "La fecha es obligatoria.")]
        public DateTime FechaReserva { get; set; } = DateTime.Today.AddDays(1);

        [Required(ErrorMessage = "La hora de inicio es obligatoria.")]
        public TimeSpan HoraInicio { get; set; } = TimeSpan.FromHours(10);

        [Required(ErrorMessage = "La hora de fin es obligatoria.")]
        public TimeSpan HoraFin { get; set; } = TimeSpan.FromHours(12);

        [StringLength(1000, ErrorMessage = "Las observaciones no pueden superar los 1000 caracteres.")]
        public string? Observaciones { get; set; }

        public List<UnidadFuncional> UnidadesFuncionales { get; set; } = new();
        public List<Amenity> Amenities { get; set; } = new();
    }
}
