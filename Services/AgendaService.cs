using Microsoft.EntityFrameworkCore;
using PortalInmobiliario.Data;
using System;
using System.Threading.Tasks;

namespace PortalInmobiliario.Services
{
    public class AgendaService
    {
        private readonly ApplicationDbContext _db;
        public AgendaService(ApplicationDbContext db) => _db = db;

        // ¿existe visita solapada para el inmueble?
        public Task<bool> ExisteSolapeVisita(int inmuebleId, DateTime ini, DateTime fin)
        {
            return _db.Visitas
                .AsNoTracking()
                .Where(v => v.InmuebleId == inmuebleId && v.Estado != Models.EstadoVisita.Cancelada)
                .AnyAsync(v => ini < v.FechaFin && fin > v.FechaInicio);
        }

        // ¿hay reserva activa (ahora < FechaExpiracion)?
        public Task<bool> TieneReservaActiva(int inmuebleId, DateTime ahoraUtc)
        {
            return _db.Reservas
                .AsNoTracking()
                .AnyAsync(r => r.InmuebleId == inmuebleId && r.FechaExpiracion > ahoraUtc);
        }
    }
}
