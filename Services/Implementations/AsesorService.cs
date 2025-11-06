using Asesorias_API_MVC.Data;
using Asesorias_API_MVC.Models;
using Asesorias_API_MVC.Models.Dtos;
using Asesorias_API_MVC.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Asesorias_API_MVC.Services.Implementations
{
    public class AsesorService : IAsesorService
    {
        private readonly ApplicationDbContext _context;
        // --- CAMBIO CLAVE ---
        // Quitamos IWebHostEnvironment, ya no se necesita.

        public AsesorService(ApplicationDbContext context)
        {
            _context = context;
        }

        // --- CAMBIO CLAVE ---
        // Actualizamos la firma del método
        public async Task<GenericResponseDto> ApplyToBeAsesorAsync(AsesorApplyDto dto, string userId)
        {
            // 1. Validar si ya existe la solicitud
            var existingApplication = await _context.Asesores.FindAsync(userId);
            if (existingApplication != null)
            {
                return new GenericResponseDto { IsSuccess = false, Message = "Ya tienes una solicitud pendiente o aprobada." };
            }

            // --- CAMBIO CLAVE ---
            // Quitamos toda la validación y guardado de archivos.

            // 2. Crear la nueva solicitud
            var newAsesorApplication = new Asesor
            {
                UsuarioId = userId,
                Especialidad = dto.Especialidad,
                Descripcion = dto.Descripcion,
                Estudios = dto.Estudios,
                Experiencia = dto.Experiencia,
                // ¡Guardamos la URL directamente desde el DTO!
                DocumentoVerificacionUrl = dto.DocumentoVerificacionUrl,
                EstaAprobado = false
            };

            // 3. Guardar en la base de datos
            await _context.Asesores.AddAsync(newAsesorApplication);
            await _context.SaveChangesAsync();

            return new GenericResponseDto
            {
                IsSuccess = true,
                Message = "¡Solicitud enviada! Un administrador la revisará pronto."
            };
        }
    }
}
