using Asesorias_API_MVC.Models;
using Asesorias_API_MVC.Models.Dtos;
using Asesorias_API_MVC.Models.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Asesorias_API_MVC.Data
{
    public class ApplicationDbContext : IdentityDbContext<Usuario, IdentityRole<int>, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Asesor> Asesores { get; set; }
        public DbSet<Curso> Cursos { get; set; }
        public DbSet<Leccion> Lecciones { get; set; }
        public DbSet<Inscripcion> Inscripciones { get; set; }
        public DbSet<SolicitudDeAyuda> SolicitudesDeAyuda { get; set; }
        public DbSet<OfertaSolicitud> OfertasSolicitud { get; set; }
        public DbSet<ProgresoLeccion> Progresos { get; set; } // La nueva tabla

        // --- INTERCEPTOR DE AUDITORÍA Y BORRADO LÓGICO ---
        private void OnBeforeSaveChanges()
        {
            var now = DateTime.UtcNow;
            var entries = ChangeTracker.Entries();

            foreach (var entry in entries)
            {
                if (entry.Entity is IAuditable auditableEntity)
                {
                    switch (entry.State)
                    {
                        case EntityState.Added:
                            auditableEntity.CreatedAt = now;
                            auditableEntity.ModifiedAt = now;
                            break;
                        case EntityState.Modified:
                            auditableEntity.ModifiedAt = now;
                            break;
                    }
                }

                if (entry.Entity is ISoftDeletable softDeletableEntity && entry.State == EntityState.Deleted)
                {
                    entry.State = EntityState.Modified;
                    softDeletableEntity.IsActive = false;
                    if (entry.Entity is IAuditable auditableOnDelete)
                    {
                        auditableOnDelete.ModifiedAt = now;
                    }
                }
            }
        }

        public override int SaveChanges()
        {
            OnBeforeSaveChanges();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            OnBeforeSaveChanges();
            return base.SaveChangesAsync(cancellationToken);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Filtros globales
            builder.Entity<Usuario>().HasQueryFilter(e => e.IsActive);
            builder.Entity<Asesor>().HasQueryFilter(e => e.IsActive);
            builder.Entity<Curso>().HasQueryFilter(e => e.IsActive);
            builder.Entity<Leccion>().HasQueryFilter(e => e.IsActive);
            builder.Entity<Inscripcion>().HasQueryFilter(e => e.IsActive);
            builder.Entity<SolicitudDeAyuda>().HasQueryFilter(e => e.IsActive);
            builder.Entity<OfertaSolicitud>().HasQueryFilter(e => e.IsActive);

            // --- CONFIGURACIÓN DE RELACIONES ---

            builder.Entity<Asesor>()
                .HasOne(a => a.Usuario)
                .WithOne(u => u.Asesor)
                .HasForeignKey<Asesor>(a => a.UsuarioId);

            // Relaciones con RESTRICT para evitar ciclos
            builder.Entity<SolicitudDeAyuda>()
                .HasOne(s => s.Estudiante)
                .WithMany(u => u.Solicitudes)
                .HasForeignKey(s => s.EstudianteId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<SolicitudDeAyuda>()
                .HasOne(s => s.AsesorAsignado)
                .WithMany(a => a.SolicitudesAtendidas)
                .HasForeignKey(s => s.AsesorAsignadoId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            builder.Entity<Inscripcion>()
                .HasOne(i => i.Estudiante)
                .WithMany(u => u.Inscripciones)
                .HasForeignKey(i => i.EstudianteId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Inscripcion>()
                .HasOne(i => i.Curso)
                .WithMany(c => c.Inscripciones)
                .HasForeignKey(i => i.CursoId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<OfertaSolicitud>()
                .HasOne(o => o.Solicitud)
                .WithMany(s => s.Ofertas)
                .HasForeignKey(o => o.SolicitudId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<OfertaSolicitud>()
                .HasOne(o => o.Asesor)
                .WithMany()
                .HasForeignKey(o => o.AsesorId)
                .OnDelete(DeleteBehavior.Restrict);

            // --- AQUÍ ESTÁ EL CAMBIO PARA CORREGIR EL ERROR ---
            builder.Entity<ProgresoLeccion>()
                .HasIndex(p => new { p.EstudianteId, p.LeccionId })
                .IsUnique();

            builder.Entity<ProgresoLeccion>()
                .HasOne(p => p.Estudiante)
                .WithMany()
                .HasForeignKey(p => p.EstudianteId)
                .OnDelete(DeleteBehavior.Restrict); // CAMBIADO A RESTRICT

            builder.Entity<ProgresoLeccion>()
                .HasOne(p => p.Leccion)
                .WithMany()
                .HasForeignKey(p => p.LeccionId)
                .OnDelete(DeleteBehavior.Restrict); // CAMBIADO A RESTRICT
            // ------------------------------------------------

            builder.Entity<AsesorRatingDto>(e =>
            {
                e.HasNoKey();
                e.Property(p => p.IngresosGenerados).HasPrecision(18, 2);
            });
        }
    }
}