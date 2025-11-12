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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Mapear el nombre de la tabla
            modelBuilder.Entity<Calificacion>().ToTable("Calificaciones");
        }
    }
}
