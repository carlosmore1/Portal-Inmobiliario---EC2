using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalInmobiliario.Data;
using PortalInmobiliario.Models;

namespace PortalInmobiliario.Areas.Broker.Controllers
{
    [Area("Broker")]
    [Authorize(Roles = "Broker")]
    public class AgendaController : Controller
    {
        private readonly ApplicationDbContext _db;
        public AgendaController(ApplicationDbContext db) { _db = db; }

        // Agenda del día (solo lectura)
        public async Task<IActionResult> Index()
        {
            var hoy = DateTime.Today;
            var manana = hoy.AddDays(1);

            var visitas = await _db.Visitas.AsNoTracking()
                .Include(v => v.Inmueble)
                .Where(v => v.FechaInicio >= hoy && v.FechaInicio < manana)
                .OrderBy(v => v.FechaInicio)
                .ToListAsync();

            return View(visitas);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirmar(int id)
        {
            var v = await _db.Visitas.FindAsync(id);
            if (v == null) return NotFound();
            v.Estado = EstadoVisita.Confirmada;
            await _db.SaveChangesAsync();
            TempData["Ok"] = "Visita confirmada.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancelar(int id)
        {
            var v = await _db.Visitas.FindAsync(id);
            if (v == null) return NotFound();
            v.Estado = EstadoVisita.Cancelada;
            await _db.SaveChangesAsync();
            TempData["Ok"] = "Visita cancelada.";
            return RedirectToAction(nameof(Index));
        }
    }
}
