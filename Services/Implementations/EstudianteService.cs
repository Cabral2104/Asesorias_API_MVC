using Asesorias_API_MVC.Data;
using Asesorias_API_MVC.Models;
using Asesorias_API_MVC.Models.Dtos;
using Asesorias_API_MVC.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Asesorias_API_MVC.Services.Implementations
{
    public class EstudianteService : IEstudianteService
    {
        // ¡Ahora inyectamos AMBOS DbContexts!
        private readonly ApplicationDbContext _appDb; // SQL Server
        private readonly AnalyticsDbContext _analyticsDb; // PostgreSQL

        public EstudianteService(ApplicationDbContext appDb, AnalyticsDbContext analyticsDb)
        {
            _appDb = appDb;
            _analyticsDb = analyticsDb;
        }

        // --- LÓGICA DE INSCRIPCIÓN ACTUALIZADA ---
        public async Task<GenericResponseDto> InscribirseACursoAsync(int cursoId, string estudianteId)
        {
            // 1. Verificamos en SQL Server que el curso existe y está publicado
            var curso = await _appDb.Cursos
                .FirstOrDefaultAsync(c => c.CursoId == cursoId && c.EstaPublicado && c.IsActive);

            if (curso == null)
            {
                return new GenericResponseDto { IsSuccess = false, Message = "Este curso no existe o no está disponible." };
            }

            // 2. Verificamos en SQL Server que el estudiante no esté ya inscrito
            var yaInscrito = await _appDb.Inscripciones
                .AnyAsync(i => i.CursoId == cursoId && i.EstudianteId == estudianteId && i.IsActive);

            if (yaInscrito)
            {
                return new GenericResponseDto { IsSuccess = false, Message = "Ya estás inscrito en este curso." };
            }

            // --- ¡NUEVO! Obtenemos datos del estudiante para la facturación ---
            var estudiante = await _appDb.Users.FindAsync(estudianteId);
            if (estudiante == null)
            {
                return new GenericResponseDto { IsSuccess = false, Message = "Error: No se encontró tu usuario." };
            }
            // --- FIN ---

            // 3. Crear la inscripción (en SQL Server)
            var nuevaInscripcion = new Inscripcion
            {
                EstudianteId = estudianteId,
                CursoId = cursoId
            };
            await _appDb.Inscripciones.AddAsync(nuevaInscripcion);

            // 4. ¡NUEVO! Crear el registro de pago simulado (en PostgreSQL)
            var nuevoPago = new HistorialPago
            {
                CursoId = cursoId,
                EstudianteId = estudianteId,
                Monto = curso.Costo,
                FechaPago = DateTime.UtcNow,
                // Simulamos los nuevos datos
                MetodoPago = "Simulado (Tarjeta)",
                CorreoFacturacion = estudiante.Email // Usamos el email del usuario
            };
            await _analyticsDb.HistorialDePagos.AddAsync(nuevoPago);

            // 5. Guardar los cambios en AMBAS bases de datos
            await _appDb.SaveChangesAsync();
            await _analyticsDb.SaveChangesAsync();

            return new GenericResponseDto { IsSuccess = true, Message = "¡Inscripción exitosa! Pago registrado." };
        }

        // --- LÓGICA PARA VER "MIS CURSOS" (SIN CAMBIOS) ---
        public async Task<IEnumerable<CursoPublicDto>> GetMisCursosAsync(string estudianteId)
        {
            var misInscripciones = await _appDb.Inscripciones
                .Where(i => i.EstudianteId == estudianteId && i.IsActive)
                .Include(i => i.Curso)
                    .ThenInclude(c => c.Asesor)
                    .ThenInclude(a => a.Usuario)
                .Select(i => new CursoPublicDto
                {
                    CursoId = i.Curso.CursoId,
                    Titulo = i.Curso.Titulo,
                    Descripcion = i.Curso.Descripcion,
                    Costo = i.Curso.Costo,
                    EstaPublicado = i.Curso.EstaPublicado,
                    AsesorId = i.Curso.AsesorId,
                    AsesorNombre = i.Curso.Asesor.Usuario.UserName
                })
                .ToListAsync();

            return misInscripciones;
        }
    }
}
