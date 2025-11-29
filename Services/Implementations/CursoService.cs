using Asesorias_API_MVC.Data;
using Asesorias_API_MVC.Models;
using Asesorias_API_MVC.Models.Dtos;
using Asesorias_API_MVC.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Asesorias_API_MVC.Services.Implementations
{
    public class CursoService : ICursoService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Usuario> _userManager;

        // Inyectamos el DbContext y el UserManager
        public CursoService(ApplicationDbContext context, UserManager<Usuario> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // --- LÓGICA PARA CREAR UN CURSO ---
        public async Task<GenericResponseDto> CreateCursoAsync(CursoCreateDto dto, int asesorId)
        {
            // Verificamos si el AsesorId (que viene del token) es válido
            var asesor = await _context.Asesores.FindAsync(asesorId);
            if (asesor == null || !asesor.EstaAprobado)
            {
                return new GenericResponseDto { IsSuccess = false, Message = "No tienes permisos para crear cursos. Asegúrate de ser un asesor aprobado." };
            }

            // Creamos el nuevo objeto Curso
            var nuevoCurso = new Curso
            {
                Titulo = dto.Titulo,
                Descripcion = dto.Descripcion,
                Costo = dto.Costo,
                AsesorId = asesorId, // Asignamos el ID del asesor desde el token
                EstaPublicado = false // Por defecto, los cursos inician como borrador
                // Los campos de auditoría (CreatedAt, IsActive) se llenan solos
            };

            await _context.Cursos.AddAsync(nuevoCurso);
            await _context.SaveChangesAsync();

            return new GenericResponseDto { IsSuccess = true, Message = "¡Curso creado exitosamente! Ahora puedes agregarle lecciones." };
        }

        // --- LÓGICA PARA VER EL CATÁLOGO PÚBLICO ---
        public async Task<IEnumerable<CursoPublicDto>> GetCursosPublicosAsync()
        {
            var cursos = await _context.Cursos
                .Where(c => c.EstaPublicado == true && c.IsActive == true) // Solo cursos activos y publicados
                .Include(c => c.Asesor)       // Carga la entidad Asesor
                    .ThenInclude(a => a.Usuario) // ¡Carga la entidad Usuario del Asesor!
                .Select(c => new CursoPublicDto
                {
                    CursoId = c.CursoId,
                    Titulo = c.Titulo,
                    Descripcion = c.Descripcion,
                    Costo = c.Costo,
                    AsesorId = c.AsesorId,
                    // Obtenemos el nombre del Usuario vinculado al Asesor
                    AsesorNombre = c.Asesor.Usuario.UserName
                })
                .ToListAsync();

            return cursos;
        }

        // --- LÓGICA PARA PUBLICAR UN CURSO ---
        public async Task<GenericResponseDto> PublishCursoAsync(int cursoId, int asesorId)
        {
            var curso = await _context.Cursos.FindAsync(cursoId);

            if (curso == null || curso.IsActive == false)
            {
                return new GenericResponseDto { IsSuccess = false, Message = "El curso no existe." };
            }

            // ¡Validación de propiedad!
            // Nos aseguramos de que el asesor que lo pide sea el dueño del curso
            if (curso.AsesorId != asesorId)
            {
                return new GenericResponseDto { IsSuccess = false, Message = "No tienes permiso para modificar este curso." };
            }

            if (curso.EstaPublicado)
            {
                return new GenericResponseDto { IsSuccess = false, Message = "Este curso ya está publicado." };
            }

            curso.EstaPublicado = true;
            await _context.SaveChangesAsync(); // El DbContext detecta el cambio y lo guarda

            return new GenericResponseDto { IsSuccess = true, Message = "¡Curso publicado exitosamente!" };
        }

        // --- NUEVO MÉTODO: Ver mis cursos ---
        public async Task<IEnumerable<CursoPublicDto>> GetMyCursosAsync(int asesorId)
        {
            var misCursos = await _context.Cursos
                .Where(c => c.AsesorId == asesorId && c.IsActive == true) // Filtra por el ID del asesor
                .Include(c => c.Asesor.Usuario) // Incluimos al usuario para obtener el nombre
                .Select(c => new CursoPublicDto
                {
                    CursoId = c.CursoId,
                    Titulo = c.Titulo,
                    Descripcion = c.Descripcion,
                    Costo = c.Costo,
                    AsesorId = c.AsesorId,
                    AsesorNombre = c.Asesor.Usuario.UserName
                    // Podríamos añadir 'EstaPublicado' al DTO si quisiéramos
                })
                .ToListAsync();

            return misCursos;
        }

        // --- NUEVO MÉTODO: Actualizar mi curso ---
        public async Task<GenericResponseDto> UpdateCursoAsync(int cursoId, CursoCreateDto dto, int asesorId)
        {
            var curso = await _context.Cursos.FindAsync(cursoId);

            if (curso == null || curso.IsActive == false)
            {
                return new GenericResponseDto { IsSuccess = false, Message = "El curso no existe." };
            }

            // --- ¡VERIFICACIÓN DE PROPIEDAD! ---
            // Nos aseguramos de que el asesor que lo pide sea el dueño del curso
            if (curso.AsesorId != asesorId)
            {
                return new GenericResponseDto { IsSuccess = false, Message = "No tienes permiso para modificar este curso." };
            }

            // Mapeamos los campos del DTO al modelo de la base de datos
            curso.Titulo = dto.Titulo;
            curso.Descripcion = dto.Descripcion;
            curso.Costo = dto.Costo;
            // 'ModifiedAt' se actualizará automáticamente por nuestro DbContext

            await _context.SaveChangesAsync();

            return new GenericResponseDto { IsSuccess = true, Message = "Curso actualizado exitosamente." };
        }

        public async Task<GenericResponseDto> DeleteCursoAsync(int cursoId, int asesorId)
        {
            var curso = await _context.Cursos
                .Include(c => c.Lecciones) // ¡IMPORTANTE: Cargar las lecciones hijas!
                .FirstOrDefaultAsync(c => c.CursoId == cursoId && c.IsActive);

            if (curso == null)
            {
                return new GenericResponseDto { IsSuccess = false, Message = "El curso no existe." };
            }

            // --- ¡VERIFICACIÓN DE PROPIEDAD! ---
            if (curso.AsesorId != asesorId)
            {
                return new GenericResponseDto { IsSuccess = false, Message = "No tienes permiso para eliminar este curso." };
            }

            // 1. Borrado lógico del Curso padre
            // (Nuestro DbContext lo interceptará y pondrá IsActive = false)
            _context.Cursos.Remove(curso);

            // 2. Borrado lógico en cascada de las Lecciones hijas
            foreach (var leccion in curso.Lecciones)
            {
                // (Nuestro DbContext también interceptará cada uno de estos)
                _context.Lecciones.Remove(leccion);
            }

            await _context.SaveChangesAsync();

            return new GenericResponseDto { IsSuccess = true, Message = "Curso y todas sus lecciones han sido eliminados (desactivados)." };
        }
    }
}
