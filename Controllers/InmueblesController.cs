using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalInmobiliario.Data;
using PortalInmobiliario.ViewModels;
using PortalInmobiliario.Models;
using PortalInmobiliario.Services; // ← agregado
using System.Linq;
using System.Threading.Tasks;
using System;

namespace PortalInmobiliario.Controllers
{
    public class InmueblesController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly AgendaService _agenda; // ← agregado

        // ← modificado para inyectar AgendaService
        public InmueblesController(ApplicationDbContext db, AgendaService agenda)
        {
            _db = db;
            _agenda = agenda;
        }

        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] CatalogoFiltroVm f)
        {
            // Ciudades para el combo (solo activas)
            f.Ciudades = await _db.Inmuebles
                .AsNoTracking()
                .Where(i => i.Activo)
                .Select(i => i.Ciudad)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            // Validaciones server-side
            if (f.PrecioMin is > 0 && f.PrecioMax is > 0 && f.PrecioMin > f.PrecioMax)
                ModelState.AddModelError(string.Empty, "El precio mínimo no puede ser mayor que el máximo.");

            if (!ModelState.IsValid)
            {
                // Si hay errores, no filtra; muestra página 1 sin resultados para no confundir
                f.Resultados = Array.Empty<Inmueble>();
                f.Total = 0;
                return View(f);
            }

            // Query base: solo activos
            var q = _db.Inmuebles.AsNoTracking().Where(i => i.Activo);

            // Aplicar filtros
            if (!string.IsNullOrWhiteSpace(f.Ciudad))
                q = q.Where(i => i.Ciudad == f.Ciudad);

            if (f.Tipo.HasValue)
                q = q.Where(i => i.Tipo == f.Tipo.Value);

            if (f.PrecioMin is > 0)
                q = q.Where(i => i.Precio >= f.PrecioMin!.Value);

            if (f.PrecioMax is > 0)
                q = q.Where(i => i.Precio <= f.PrecioMax!.Value);

            if (f.DormitoriosMin is > 0)
                q = q.Where(i => i.Dormitorios >= f.DormitoriosMin!.Value);

            // Total para paginación
            f.Total = await q.CountAsync();

            // Paginación simple
            var skip = (f.Page - 1) * f.PageSize;
            f.Resultados = await q
                .OrderBy(i => i.Precio)
                .Skip(skip)
                .Take(f.PageSize)
                .ToListAsync();

            return View(f);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var inm = await _db.Inmuebles.AsNoTracking()
                .FirstOrDefaultAsync(i => i.Id == id && i.Activo);

            if (inm == null) return NotFound();

            // ← agregado: flag para ocultar/mostrar el botón "Reservar ahora"
            ViewBag.HasReservaActiva = await _agenda.TieneReservaActiva(id, DateTime.UtcNow);

            return View(inm);
        }
    }
}
