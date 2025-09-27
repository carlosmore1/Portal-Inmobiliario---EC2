using System.ComponentModel.DataAnnotations;
using PortalInmobiliario.Models;
using System.Collections.Generic;

namespace PortalInmobiliario.ViewModels
{
    public class CatalogoFiltroVm
    {
        // Filtros
        public string? Ciudad { get; set; }
        public TipoInmueble? Tipo { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Precio mínimo no puede ser negativo")]
        public decimal? PrecioMin { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Precio máximo no puede ser negativo")]
        public decimal? PrecioMax { get; set; }

        [Range(0, 50, ErrorMessage = "Dormitorios no puede ser negativo")]
        public int? DormitoriosMin { get; set; }

        // Paginación
        [Range(1, int.MaxValue)]
        public int Page { get; set; } = 1;

        [Range(1, 100)]
        public int PageSize { get; set; } = 10;

        // Resultados
        public IEnumerable<Inmueble> Resultados { get; set; } = new List<Inmueble>();
        public int Total { get; set; }

        // Para combos
        public IEnumerable<string> Ciudades { get; set; } = new List<string>();
    }
}
