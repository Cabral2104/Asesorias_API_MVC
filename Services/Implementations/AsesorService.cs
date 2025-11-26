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
        private readonly IWebHostEnvironment _env; // Inyectamos entorno para saber ruta wwwroot

        public AsesorService(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<GenericResponseDto> ApplyToBeAsesorAsync(AsesorApplyDto dto, string userId)
        {
            var existingApplication = await _context.Asesores.FindAsync(userId);
            if (existingApplication != null)
            {
                return new GenericResponseDto { IsSuccess = false, Message = "Ya tienes una solicitud pendiente o aprobada." };
            }

            // Guardar Archivo
            string fileUrl = "";
            if (dto.DocumentoVerificacion != null && dto.DocumentoVerificacion.Length > 0)
            {
                // 1. Definir ruta: wwwroot/Uploads/CVs
                string uploadsFolder = Path.Combine(_env.WebRootPath, "Uploads", "CVs");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                // 2. Nombre único
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + dto.DocumentoVerificacion.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // 3. Guardar
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.DocumentoVerificacion.CopyToAsync(fileStream);
                }

                // 4. Guardar URL relativa para la BD
                fileUrl = "/Uploads/CVs/" + uniqueFileName;
            }

            var newAsesor = new Asesor
            {
                UsuarioId = userId,
                Especialidad = dto.Especialidad,
                Descripcion = dto.Descripcion,
                NivelEstudios = dto.NivelEstudios,
                InstitucionEducativa = dto.InstitucionEducativa,
                CampoEstudio = dto.CampoEstudio,
                AnioGraduacion = dto.AnioGraduacion,
                AniosExperiencia = dto.AniosExperiencia,
                ExperienciaLaboral = dto.ExperienciaLaboral,
                Certificaciones = dto.Certificaciones,
                DocumentoVerificacionUrl = fileUrl, // Guardamos la ruta del archivo
                EstaAprobado = false
            };

            await _context.Asesores.AddAsync(newAsesor);
            await _context.SaveChangesAsync();

            return new GenericResponseDto { IsSuccess = true, Message = "Solicitud enviada correctamente." };
        }
    }
}
