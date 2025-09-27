using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalInmobiliario.Data;

namespace PortalInmobiliario.Areas.Broker.Controllers
{
    [Area("Broker")]
    [Authorize(Roles = "Broker")]
    public class ReservasController : Controller
    {
        private readonly ApplicationDbContext _db;
        public ReservasController(ApplicationDbContext db) { _db = db; }

        // Reservas activas
        public async Task<IActionResult> Index()
        {
            var ahora = DateTime.UtcNow;
            var list = await _db.Reservas.AsNoTracking()
                .Include(r => r.Inmueble)
                .Where(r => r.FechaExpiracion > ahora)
                .OrderBy(r => r.FechaExpiracion)
                .ToListAsync();

            return View(list);
        }

        // “Liberar” = cancelar/eliminar reserva activa
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Liberar(int id)
        {
            var r = await _db.Reservas.FindAsync(id);
            if (r == null) return NotFound();

            _db.Reservas.Remove(r);
            await _db.SaveChangesAsync();

            TempData["Ok"] = "Reserva liberada.";
            return RedirectToAction(nameof(Index));
        }
    }
}
