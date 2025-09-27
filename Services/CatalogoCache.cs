using Microsoft.Extensions.Caching.Distributed;
using Microsoft.EntityFrameworkCore;
using PortalInmobiliario.Data;
using PortalInmobiliario.Models;
using PortalInmobiliario.ViewModels;
using System.Text.Json;

namespace PortalInmobiliario.Services
{
    public class CatalogoCache
    {
        private readonly IDistributedCache _cache;
        private readonly ApplicationDbContext _db;
        private readonly ILogger<CatalogoCache> _log;

        private const string VersionKey = "Cat:Version"; // para invalidación

        public CatalogoCache(IDistributedCache cache, ApplicationDbContext db, ILogger<CatalogoCache> log)
        {
            _cache = cache; _db = db; _log = log;
        }

        private static string Key(CatalogoFiltroVm f, string ver) =>
            $"Cat:List:v={ver}|c={f.Ciudad}|t={f.Tipo}|pmin={f.PrecioMin}|pmax={f.PrecioMax}|dmin={f.DormitoriosMin}|pg={f.Page}|ps={f.PageSize}";

        public async Task<(IEnumerable<Inmueble> datos, int total)> Listar(CatalogoFiltroVm f)
        {
            var ver = await _cache.GetStringAsync(VersionKey) ?? "1";
            var key = Key(f, ver);

            var cached = await _cache.GetStringAsync(key);
            if (cached is not null)
            {
                try
                {
                    var dto = JsonSerializer.Deserialize<Payload>(cached);
                    if (dto != null) return (dto.Datos, dto.Total);
                }
                catch { /* si falla deserializar, continúa */ }
            }

            // Construye la query igual que en P2
            var q = _db.Inmuebles.AsNoTracking().Where(i => i.Activo);
            if (!string.IsNullOrWhiteSpace(f.Ciudad)) q = q.Where(i => i.Ciudad == f.Ciudad);
            if (f.Tipo.HasValue) q = q.Where(i => i.Tipo == f.Tipo);
            if (f.PrecioMin is > 0) q = q.Where(i => i.Precio >= f.PrecioMin);
            if (f.PrecioMax is > 0) q = q.Where(i => i.Precio <= f.PrecioMax);
            if (f.DormitoriosMin is > 0) q = q.Where(i => i.Dormitorios >= f.DormitoriosMin);

            var total = await q.CountAsync();
            var datos = await q.OrderBy(i => i.Precio)
                               .Skip((f.Page - 1) * f.PageSize)
                               .Take(f.PageSize)
                               .ToListAsync();

            var payload = JsonSerializer.Serialize(new Payload { Datos = datos, Total = total });
            await _cache.SetStringAsync(key, payload, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60)
            });

            return (datos, total);
        }

        public async Task Invalidar()
        {
            // cambia la “versión” para invalidar todas las entradas del catálogo
            await _cache.SetStringAsync(VersionKey, Guid.NewGuid().ToString("N"));
        }

        private class Payload
        {
            public List<Inmueble> Datos { get; set; } = new();
            public int Total { get; set; }
        }
    }
}
