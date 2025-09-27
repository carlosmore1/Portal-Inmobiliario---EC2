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
    public class VisitasController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly AgendaService _agenda;
        private readonly UserManager<IdentityUser> _um;

        public VisitasController(ApplicationDbContext db, AgendaService agenda, UserManager<IdentityUser> um)
        {
            _db = db; _agenda = agenda; _um = um;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Agendar(int InmuebleId, DateTime FechaInicio, DateTime FechaFin, string? Notas)
        {
            // Validación básica
            if (FechaInicio >= FechaFin)
            {
                TempData["Err"] = "La fecha de inicio debe ser menor a la de fin.";
                return RedirectToAction("Details", "Inmuebles", new { id = InmuebleId });
            }

            // Horario laboral 08:00–19:00 (según consigna)
            bool DentroHorario(TimeSpan t) => t >= new TimeSpan(8,0,0) && t <= new TimeSpan(19,0,0);
            if (!DentroHorario(FechaInicio.TimeOfDay) || !DentroHorario(FechaFin.TimeOfDay))
            {
                TempData["Err"] = "Las visitas solo se permiten entre 08:00 y 19:00.";
                return RedirectToAction("Details", "Inmuebles", new { id = InmuebleId });
            }

            // Verificar existencia del inmueble activo
            var inm = await _db.Inmuebles.AsNoTracking().FirstOrDefaultAsync(i => i.Id == InmuebleId && i.Activo);
            if (inm == null)
            {
                TempData["Err"] = "El inmueble no existe o no está activo.";
                return RedirectToAction("Details", "Inmuebles", new { id = InmuebleId });
            }

            // Solape
            if (await _agenda.ExisteSolapeVisita(InmuebleId, FechaInicio, FechaFin))
            {
                TempData["Err"] = "Ya existe una visita en ese intervalo para este inmueble.";
                return RedirectToAction("Details", "Inmuebles", new { id = InmuebleId });
            }

            var userId = _um.GetUserId(User)!;
            var visita = new Visita
            {
                InmuebleId = InmuebleId,
                UsuarioId = userId,
                FechaInicio = FechaInicio,
                FechaFin = FechaFin,
                Estado = EstadoVisita.Solicitada,
                Notas = string.IsNullOrWhiteSpace(Notas) ? null : Notas.Trim()
            };

            _db.Visitas.Add(visita);
            await _db.SaveChangesAsync();

            TempData["Ok"] = "Visita solicitada correctamente.";
            return RedirectToAction("Details", "Inmuebles", new { id = InmuebleId });
        }
    }
}
