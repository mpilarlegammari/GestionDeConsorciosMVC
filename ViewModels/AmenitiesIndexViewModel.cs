namespace GestionDeConsorciosMVC.ViewModels
{
    public class AmenitiesIndexViewModel
    {
        public List<Amenity> Amenities { get; set; } = new();
        public List<Consorcio> Consorcios { get; set; } = new();

        public int? ConsorcioId { get; set; }
        public bool? Activo { get; set; }
        public string? Busqueda { get; set; }

        public int CantidadAmenities => Amenities.Count;
        public int CantidadActivos => Amenities.Count(amenity => amenity.Activo);
        public int CapacidadTotal => Amenities.Where(amenity => amenity.Activo).Sum(amenity => amenity.Capacidad);
    }
}
