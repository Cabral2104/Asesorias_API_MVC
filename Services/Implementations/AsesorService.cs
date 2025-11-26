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
        private readonly IWebHostEnvironment _env;

        public AsesorService(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<GenericResponseDto> ApplyToBeAsesorAsync(AsesorApplyDto dto, string userId)
        {
            // 1. Verificar si ya existe
            var existingApplication = await _context.Asesores.FindAsync(userId);
            if (existingApplication != null)
            {
                return new GenericResponseDto { IsSuccess = false, Message = "Ya tienes una solicitud registrada." };
            }

            string fileUrl = "";

            // 2. PROCESAR BASE64
            if (!string.IsNullOrEmpty(dto.ArchivoBase64))
            {
                try
                {
                    // Ruta segura: usa ContentRoot si WebRoot es nulo
                    string webRootPath = _env.WebRootPath ?? _env.ContentRootPath;
                    if (string.IsNullOrEmpty(webRootPath))
                        webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

                    string uploadsFolder = Path.Combine(webRootPath, "Uploads", "CVs");
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                    // Limpiar encabezado del base64 si existe (data:application/pdf;base64,...)
                    string base64Clean = dto.ArchivoBase64;
                    if (base64Clean.Contains(","))
                    {
                        base64Clean = base64Clean.Substring(base64Clean.IndexOf(",") + 1);
                    }

                    // Convertir a bytes
                    byte[] fileBytes = Convert.FromBase64String(base64Clean);

                    // Generar nombre único
                    string extension = Path.GetExtension(dto.NombreArchivo);
                    if (string.IsNullOrEmpty(extension)) extension = ".pdf"; // Default

                    string uniqueName = $"{Guid.NewGuid()}{extension}";
                    string filePath = Path.Combine(uploadsFolder, uniqueName);

                    // Guardar en disco
                    await File.WriteAllBytesAsync(filePath, fileBytes);

                    // Ruta para la BD
                    fileUrl = $"/Uploads/CVs/{uniqueName}";
                }
                catch (Exception ex)
                {
                    return new GenericResponseDto { IsSuccess = false, Message = "Error al guardar archivo: " + ex.Message };
                }
            }

            // 3. Guardar en Base de Datos
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
                DocumentoVerificacionUrl = fileUrl,
                EstaAprobado = false,
                // Campos obligatorios de auditoría (si no los pone el DBContext automáticamente)
                CreatedAt = DateTime.UtcNow,
                ModifiedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _context.Asesores.AddAsync(newAsesor);
            await _context.SaveChangesAsync();

            return new GenericResponseDto { IsSuccess = true, Message = "Solicitud enviada correctamente." };
        }
    }
}
