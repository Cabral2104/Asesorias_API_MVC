using Asesorias_API_MVC.Models; // IMPORTANTE: Apunta a nuestra carpeta de Modelos
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


        // --- INTERCEPTOR DE BORRADO LÓGICO ---

        // Este método privado hace el trabajo sucio
        private void HandleSoftDelete()
        {
            // Obtener todas las entidades que están siendo 'Borradas'
            // y que implementan nuestra interfaz ISoftDeletable
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is ISoftDeletable && e.State == EntityState.Deleted);

            // Iterar sobre cada una
            foreach (var entry in entries)
            {
                // 1. Cambiar su estado de 'Borrado' a 'Modificado'
                entry.State = EntityState.Modified;

                // 2. Poner la propiedad IsActive en 'false'
                ((ISoftDeletable)entry.Entity).IsActive = false;
            }
        }

        // Sobrescribir el método SaveChanges() síncrono
        public override int SaveChanges()
        {
            HandleSoftDelete(); // Llamar a nuestro interceptor
            return base.SaveChanges();
        }

        // Sobrescribir el método SaveChangesAsync() asíncrono
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            HandleSoftDelete(); // Llamar a nuestro interceptor
            return base.SaveChangesAsync(cancellationToken);
        }

        // --- FIN DEL INTERCEPTOR ---


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // --- APLICAR FILTROS GLOBALES DE CONSULTA ---
            // Esto hace que CUALQUIER consulta (ToList(), FindAsync(), etc.)
            // ignore automáticamente los registros con IsActive = false.
            builder.Entity<Usuario>().HasQueryFilter(e => e.IsActive);
            builder.Entity<Asesor>().HasQueryFilter(e => e.IsActive);
            builder.Entity<Curso>().HasQueryFilter(e => e.IsActive);
            builder.Entity<Leccion>().HasQueryFilter(e => e.IsActive);
            builder.Entity<Inscripcion>().HasQueryFilter(e => e.IsActive);
            builder.Entity<SolicitudDeAyuda>().HasQueryFilter(e => e.IsActive);

            // --- FIN DE FILTROS GLOBALES ---


            // --- CONFIGURACIÓN DE RELACIONES (CON BORRADO SEGURO) ---

            // Relación 1-a-1 Usuario -> Asesor
            builder.Entity<Asesor>()
                .HasOne(a => a.Usuario)
                .WithOne(u => u.Asesor)
                .HasForeignKey<Asesor>(a => a.UsuarioId);

            // Relaciones de SolicitudDeAyuda
            builder.Entity<SolicitudDeAyuda>()
                .HasOne(s => s.Estudiante)
                .WithMany(u => u.Solicitudes)
                .HasForeignKey(s => s.EstudianteId)
                .OnDelete(DeleteBehavior.Restrict); // No permitir borrado físico

            builder.Entity<SolicitudDeAyuda>()
                .HasOne(s => s.AsesorAsignado)
                .WithMany(a => a.SolicitudesAtendidas)
                .HasForeignKey(s => s.AsesorAsignadoId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            // Relaciones de Inscripcion
            builder.Entity<Inscripcion>()
                .HasOne(i => i.Estudiante)
                .WithMany(u => u.Inscripciones)
                .HasForeignKey(i => i.EstudianteId)
                .OnDelete(DeleteBehavior.Restrict); // No permitir borrado físico

            builder.Entity<Inscripcion>()
                .HasOne(i => i.Curso)
                .WithMany(c => c.Inscripciones)
                .HasForeignKey(i => i.CursoId)
                .OnDelete(DeleteBehavior.Restrict); // No permitir borrado físico
        }
    }
}
