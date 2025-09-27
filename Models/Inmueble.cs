using System.ComponentModel.DataAnnotations;

namespace PortalInmobiliario.Models
{
    public class Inmueble
    {
        public int Id { get; set; }

        [Required, StringLength(32)]
        public string Codigo { get; set; } = default!; // Único

        [Required, StringLength(120)]
        public string Titulo { get; set; } = default!;

        [StringLength(300)]
        public string? Imagen { get; set; }

        [Required]
        public TipoInmueble Tipo { get; set; }

        [Required, StringLength(80)]
        public string Ciudad { get; set; } = default!;

        [Required, StringLength(160)]
        public string Direccion { get; set; } = default!;

        [Range(0, 50)]
        public int Dormitorios { get; set; }

        [Range(0, 50)]
        public int Banos { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Metros cuadrados debe ser > 0")]
        public double MetrosCuadrados { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Precio debe ser > 0")]
        public decimal Precio { get; set; }

        public bool Activo { get; set; } = true;

        public ICollection<Visita> Visitas { get; set; } = new List<Visita>();
        public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
    }
}
