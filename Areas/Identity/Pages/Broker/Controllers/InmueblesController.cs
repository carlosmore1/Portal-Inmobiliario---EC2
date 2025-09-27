using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalInmobiliario.Data;
using PortalInmobiliario.Models;
using PortalInmobiliario.Services;

namespace PortalInmobiliario.Areas.Broker.Controllers
{
    [Area("Broker")]
    [Authorize(Roles = "Broker")]
    public class InmueblesController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly CatalogoCache _cache; // invalidar listado cacheado

        public InmueblesController(ApplicationDbContext db, CatalogoCache cache)
        {
            _db = db; _cache = cache;
        }

        public async Task<IActionResult> Index()
        {
            var list = await _db.Inmuebles
                .OrderBy(i => i.Ciudad).ThenBy(i => i.Titulo)
                .ToListAsync();
            return View(list);
        }

        [HttpGet]
        public IActionResult Create() => View(new Inmueble());

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Inmueble m)
        {
            if (!ModelState.IsValid) return View(m);
            _db.Inmuebles.Add(m);
            await _db.SaveChangesAsync();
            await _cache.Invalidar();
            TempData["Ok"] = "Inmueble creado.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var m = await _db.Inmuebles.FindAsync(id);
            if (m == null) return NotFound();
            return View(m);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Inmueble m)
        {
            if (id != m.Id) return BadRequest();
            if (!ModelState.IsValid) return View(m);

            _db.Entry(m).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            await _cache.Invalidar();
            TempData["Ok"] = "Inmueble actualizado.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Activar(int id, bool activo)
        {
            var m = await _db.Inmuebles.FindAsync(id);
            if (m == null) return NotFound();
            m.Activo = activo;
            await _db.SaveChangesAsync();
            await _cache.Invalidar();
            TempData["Ok"] = activo ? "Inmueble activado." : "Inmueble desactivado.";
            return RedirectToAction(nameof(Index));
        }
    }
}
