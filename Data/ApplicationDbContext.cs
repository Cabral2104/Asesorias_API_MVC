using Asesorias_API_MVC.Models; // IMPORTANTE: Apunta a nuestra carpeta de Modelos
using Asesorias_API_MVC.Models.Dtos;
using Asesorias_API_MVC.Models.Interfaces;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Asesorias_API_MVC.Data
{
    public class ApplicationDbContext : IdentityDbContext<Usuario>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // Registramos todas nuestras tablas
        public DbSet<Asesor> Asesores { get; set; }
        public DbSet<Curso> Cursos { get; set; }
        public DbSet<Leccion> Lecciones { get; set; }
        public DbSet<Inscripcion> Inscripciones { get; set; }
        public DbSet<SolicitudDeAyuda> SolicitudesDeAyuda { get; set; }


        // --- INTERCEPTOR AUTOMÁTICO DE BORRADO Y AUDITORÍA ---

        private void OnBeforeSaveChanges()
        {
            var now = DateTime.UtcNow; // Obtenemos la fecha una sola vez

            // Obtenemos todas las entradas que están siendo rastreadas por EF Core
            var entries = ChangeTracker.Entries();

            foreach (var entry in entries)
            {
                // ---- MANEJO DE AUDITORÍA (IAuditable) ----
                if (entry.Entity is IAuditable auditableEntity)
                {
                    switch (entry.State)
                    {
                        // Si la entidad se está AÑADIENDO
                        case EntityState.Added:
                            auditableEntity.CreatedAt = now;
                            auditableEntity.ModifiedAt = now;
                            break;

                        // Si la entidad se está MODIFICANDO
                        case EntityState.Modified:
                            auditableEntity.ModifiedAt = now;
                            break;
                    }
                }

                // ---- MANEJO DE BORRADO LÓGICO (ISoftDeletable) ----
                if (entry.Entity is ISoftDeletable softDeletableEntity && entry.State == EntityState.Deleted)
                {
                    // 1. Cambiar su estado de 'Borrado' a 'Modificado'
                    entry.State = EntityState.Modified;

                    // 2. Poner la propiedad IsActive en 'false'
                    softDeletableEntity.IsActive = false;

                    // 3. (Opcional pero bueno) Actualizar ModifiedAt al borrar lógicamente
                    if (entry.Entity is IAuditable auditableOnDelete)
                    {
                        auditableOnDelete.ModifiedAt = now;
                    }
                }
            }
        }

        // Sobrescribir el método SaveChanges() síncrono
        public override int SaveChanges()
        {
            OnBeforeSaveChanges(); // Llamar a nuestro interceptor
            return base.SaveChanges();
        }

        // Sobrescribir el método SaveChangesAsync() asíncrono
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            OnBeforeSaveChanges(); // Llamar a nuestro interceptor
            return base.SaveChangesAsync(cancellationToken);
        }

        // --- FIN DEL INTERCEPTOR ---


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // --- APLICAR FILTROS GLOBALES DE CONSULTA (Solo IsActive) ---
            builder.Entity<Usuario>().HasQueryFilter(e => e.IsActive);
            builder.Entity<Asesor>().HasQueryFilter(e => e.IsActive);
            builder.Entity<Curso>().HasQueryFilter(e => e.IsActive);
            builder.Entity<Leccion>().HasQueryFilter(e => e.IsActive);
            builder.Entity<Inscripcion>().HasQueryFilter(e => e.IsActive);
            builder.Entity<SolicitudDeAyuda>().HasQueryFilter(e => e.IsActive);

            // --- CONFIGURACIÓN DE RELACIONES (CON BORRADO SEGURO) ---

            builder.Entity<Asesor>()
                .HasOne(a => a.Usuario)
                .WithOne(u => u.Asesor)
                .HasForeignKey<Asesor>(a => a.UsuarioId);

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

            builder.Entity<AsesorRatingDto>(e =>
            {
                e.HasNoKey(); // ¡Importante! No es una tabla real
                // Le decimos qué precisión usar para el dinero
                e.Property(p => p.IngresosGenerados).HasPrecision(18, 2);
            });
        }
    }
}
