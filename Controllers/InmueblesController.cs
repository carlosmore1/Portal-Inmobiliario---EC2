using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalInmobiliario.Data;
using PortalInmobiliario.ViewModels;
using PortalInmobiliario.Models;
using PortalInmobiliario.Services; // AgendaService + CatalogoCache
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Text.Json;            // ← para serializar filtros (sesión)
using Microsoft.AspNetCore.Http;   // ← para HttpContext.Session.*

namespace PortalInmobiliario.Controllers
{
    public class InmueblesController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly AgendaService _agenda;       // P3
        private readonly CatalogoCache _catCache;     // ← P4 (Redis)

        // Claves de sesión (P4)
        private const string SessFiltros = "Cat:Filtros";
        private const string SessUltimoId = "Cat:UltimoId";
        private const string SessUltimoTitulo = "Cat:UltimoTitulo";

        // ← inyectamos CatalogoCache además de AgendaService
        public InmueblesController(ApplicationDbContext db, AgendaService agenda, CatalogoCache catCache)
        {
            _db = db;
            _agenda = agenda;
            _catCache = catCache;
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

            // ===== P4: usar caché Redis (60s) según filtros =====
            var (datos, total) = await _catCache.Listar(f);
            f.Resultados = datos;
            f.Total = total;

            // ===== P4: guardar filtros en sesión (sin Page/PageSize) =====
            var filtrosSnapshot = new
            {
                f.Ciudad,
                f.Tipo,
                f.PrecioMin,
                f.PrecioMax,
                f.DormitoriosMin
            };
            HttpContext.Session.SetString(SessFiltros, JsonSerializer.Serialize(filtrosSnapshot));

            return View(f);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var inm = await _db.Inmuebles.AsNoTracking()
                .FirstOrDefaultAsync(i => i.Id == id && i.Activo);

            if (inm == null) return NotFound();

            // P3: flag para ocultar/mostrar el botón "Reservar ahora"
            ViewBag.HasReservaActiva = await _agenda.TieneReservaActiva(id, DateTime.UtcNow);

            // ===== P4: guardar "último inmueble" en sesión =====
            HttpContext.Session.SetInt32(SessUltimoId, id);
            HttpContext.Session.SetString(SessUltimoTitulo, inm.Titulo);

            return View(inm);
        }
    }
}
