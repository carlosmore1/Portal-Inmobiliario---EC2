using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalInmobiliario.Data;
using PortalInmobiliario.Models;
using PortalInmobiliario.Services;
using System;
using System.Threading.Tasks;

namespace PortalInmobiliario.Controllers
{
    [Authorize]
    public class ReservasController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly AgendaService _agenda;
        private readonly UserManager<IdentityUser> _um;

        public ReservasController(ApplicationDbContext db, AgendaService agenda, UserManager<IdentityUser> um)
        {
            _db = db; _agenda = agenda; _um = um;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reservar(int inmuebleId)
        {
            var ahora = DateTime.UtcNow;

            // Inmueble activo
            var inm = await _db.Inmuebles.AsNoTracking().FirstOrDefaultAsync(i => i.Id == inmuebleId && i.Activo);
            if (inm == null)
            {
                TempData["Err"] = "El inmueble no existe o no está activo.";
                return RedirectToAction("Details", "Inmuebles", new { id = inmuebleId });
            }

            // Reserva activa ya existente
            if (await _agenda.TieneReservaActiva(inmuebleId, ahora))
            {
                TempData["Err"] = "El inmueble ya tiene una reserva activa.";
                return RedirectToAction("Details", "Inmuebles", new { id = inmuebleId });
            }

            var userId = _um.GetUserId(User)!;

            _db.Reservas.Add(new Reserva
            {
                InmuebleId = inmuebleId,
                UsuarioId = userId,
                FechaCreacion = ahora,
                FechaExpiracion = ahora.AddHours(48)
            });

            await _db.SaveChangesAsync();
            TempData["Ok"] = "Reserva creada por 48 horas.";
            return RedirectToAction("Details", "Inmuebles", new { id = inmuebleId });
        }
    }
}
