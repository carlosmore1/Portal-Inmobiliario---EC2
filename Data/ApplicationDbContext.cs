using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PortalInmobiliario.Models;

namespace PortalInmobiliario.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Inmueble> Inmuebles => Set<Inmueble>();
        public DbSet<Visita> Visitas => Set<Visita>();
        public DbSet<Reserva> Reservas => Set<Reserva>();

        protected override void OnModelCreating(ModelBuilder b)
        {
            base.OnModelCreating(b);

            b.Entity<Inmueble>()
                .HasIndex(i => i.Codigo)
                .IsUnique();

            b.Entity<Inmueble>().ToTable(tb =>
            {
                tb.HasCheckConstraint("CK_Inmueble_Precio_Pos", "Precio > 0");
                tb.HasCheckConstraint("CK_Inmueble_Metros_Pos", "MetrosCuadrados > 0");
            });

            b.Entity<Visita>().ToTable(tb =>
            {
                tb.HasCheckConstraint("CK_Visita_RangoValido", "FechaInicio < FechaFin");
            });

            b.Entity<Visita>()
                .HasOne(v => v.Inmueble)
                .WithMany(i => i.Visitas)
                .HasForeignKey(v => v.InmuebleId)
                .OnDelete(DeleteBehavior.Cascade);

            b.Entity<Reserva>()
                .HasOne(r => r.Inmueble)
                .WithMany(i => i.Reservas)
                .HasForeignKey(r => r.InmuebleId)
                .OnDelete(DeleteBehavior.Cascade);

            b.Entity<Inmueble>().HasData(
                new Inmueble {
                    Id = 1, Codigo = "DEP-001", Titulo = "Departamento céntrico",
                    Tipo = TipoInmueble.Departamento, Ciudad = "Lima", Direccion = "Av. Central 123",
                    Dormitorios = 2, Banos = 2, MetrosCuadrados = 75, Precio = 120000, Activo = true, Imagen = null
                },
                new Inmueble {
                    Id = 2, Codigo = "CAS-010", Titulo = "Casa familiar con patio",
                    Tipo = TipoInmueble.Casa, Ciudad = "Arequipa", Direccion = "Jr. Flores 456",
                    Dormitorios = 3, Banos = 3, MetrosCuadrados = 140, Precio = 240000, Activo = true
                },
                new Inmueble {
                    Id = 3, Codigo = "OFI-021", Titulo = "Oficina moderna",
                    Tipo = TipoInmueble.Oficina, Ciudad = "Lima", Direccion = "Calle Tech 789",
                    Dormitorios = 0, Banos = 1, MetrosCuadrados = 55, Precio = 95000, Activo = true
                },
                new Inmueble {
                    Id = 4, Codigo = "LOC-115", Titulo = "Local comercial en esquina",
                    Tipo = TipoInmueble.Local, Ciudad = "Trujillo", Direccion = "Esq. Comercio 15",
                    Dormitorios = 0, Banos = 1, MetrosCuadrados = 90, Precio = 180000, Activo = true
                }
            );
        }
    }
}
