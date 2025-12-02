using Asesorias_API_MVC.Models;
using Microsoft.EntityFrameworkCore;

namespace Asesorias_API_MVC.Data
{
    // ¡Este es un DbContext separado SOLO para PostgreSQL!
    public class AnalyticsDbContext : DbContext
    {
        public AnalyticsDbContext(DbContextOptions<AnalyticsDbContext> options) : base(options)
        {
        }

        public DbSet<Calificacion> Calificaciones { get; set; }

        public DbSet<HistorialPago> HistorialDePagos { get; set; }

        public DbSet<AuditoriaAccion> Auditorias { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Calificacion>().ToTable("Calificaciones");

            // --- MAPEO EXPLÍCITO DE LA TABLA ACTUALIZADA ---
            modelBuilder.Entity<HistorialPago>(entity =>
            {
                entity.ToTable("HistorialDePagos");
                // Definimos la precisión del decimal, buena práctica
                entity.Property(e => e.Monto).HasColumnType("decimal(10, 2)");
            });

            // Mapeo de Auditoría
            modelBuilder.Entity<AuditoriaAccion>().ToTable("AuditoriaAcciones");
        }
    }
}
