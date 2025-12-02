using Asesorias_API_MVC.Data;
using Asesorias_API_MVC.Models;
using Asesorias_API_MVC.Models.Dtos;
using Asesorias_API_MVC.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Asesorias_API_MVC.Services.Implementations
{
    public class EstudianteService : IEstudianteService
    {
        private readonly ApplicationDbContext _appDb;
        private readonly AnalyticsDbContext _analyticsDb;
        private readonly IEmailService _emailService; // Inyectamos el servicio de email

        public EstudianteService(ApplicationDbContext appDb, AnalyticsDbContext analyticsDb, IEmailService emailService)
        {
            _appDb = appDb;
            _analyticsDb = analyticsDb;
            _emailService = emailService;
        }

        public async Task<GenericResponseDto> InscribirseACursoAsync(InscripcionPagoDto dto, int estudianteId)
        {
            // 1. VALIDACIONES
            var curso = await _appDb.Cursos
                .Include(c => c.Asesor.Usuario) // Incluimos al asesor para mostrarlo en el correo
                .FirstOrDefaultAsync(c => c.CursoId == dto.CursoId && c.EstaPublicado && c.IsActive);

            if (curso == null) return new GenericResponseDto { IsSuccess = false, Message = "Curso no disponible." };

            var yaInscrito = await _appDb.Inscripciones
                .AnyAsync(i => i.CursoId == dto.CursoId && i.EstudianteId == estudianteId && i.IsActive);

            if (yaInscrito) return new GenericResponseDto { IsSuccess = false, Message = "Ya estás inscrito." };

            var estudiante = await _appDb.Users.FindAsync(estudianteId);
            if (estudiante == null) return new GenericResponseDto { IsSuccess = false, Message = "Usuario no encontrado." };

            // 2. PROCESAMIENTO SEGURO (TOKENIZACIÓN SIMULADA)
            // En un sistema real, aquí llamaríamos a Stripe/PayPal.
            // Nosotros solo guardaremos los últimos 4 dígitos por seguridad.
            string ultimosDigitos = dto.NumeroTarjeta.Substring(dto.NumeroTarjeta.Length - 4);
            string metodoPagoSeguro = $"Tarjeta terminada en **** {ultimosDigitos}";

            // 3. GUARDAR EN SQL SERVER (Inscripción)
            var nuevaInscripcion = new Inscripcion
            {
                EstudianteId = estudianteId,
                CursoId = dto.CursoId
            };
            await _appDb.Inscripciones.AddAsync(nuevaInscripcion);

            // 4. GUARDAR EN POSTGRESQL (Historial Financiero)
            var nuevoPago = new HistorialPago
            {
                CursoId = dto.CursoId,
                EstudianteId = estudianteId,
                Monto = curso.Costo,
                FechaPago = DateTime.UtcNow,
                MetodoPago = metodoPagoSeguro, // Guardamos dato seguro
                CorreoFacturacion = estudiante.Email
            };
            await _analyticsDb.HistorialDePagos.AddAsync(nuevoPago);

            // Guardar cambios en ambas BD
            await _appDb.SaveChangesAsync();
            await _analyticsDb.SaveChangesAsync();

            // 5. ENVIAR CORREO DE CONFIRMACIÓN (Asíncrono)
            // No esperamos el await para no bloquear la respuesta al usuario si el SMTP tarda
            _ = EnviarCorreoConfirmacion(estudiante.Email, estudiante.NombreCompleto, curso, metodoPagoSeguro);

            return new GenericResponseDto { IsSuccess = true, Message = "¡Pago exitoso! Te hemos enviado el recibo por correo." };
        }

        // Método privado para construir el HTML del correo
        private async Task EnviarCorreoConfirmacion(string email, string nombre, Curso curso, string metodoPago)
        {
            string asunto = "Confirmación de Inscripción - Lumina";
            // NOTA: Asegúrate que el puerto 5173 es el correcto. Si tu Vite corre en otro (ej: 5174), cámbialo aquí.
            string linkPerfil = "http://localhost:5173/profile";

            string mensaje = $@"
                <div style='font-family: Arial, sans-serif; color: #333; max-width: 600px; margin: 0 auto; border: 1px solid #e5e7eb; border-radius: 8px; overflow: hidden;'>
                    <div style='background-color: #4F46E5; padding: 20px; text-align: center;'>
                        <h1 style='color: white; margin: 0;'>¡Bienvenido a bordo!</h1>
                    </div>
                    <div style='padding: 30px;'>
                        <p>Hola <strong>{nombre}</strong>,</p>
                        <p>Tu inscripción al curso ha sido confirmada exitosamente. Aquí tienes los detalles de tu compra:</p>
                        
                        <div style='background-color: #f9fafb; padding: 15px; border-radius: 8px; margin: 20px 0;'>
                            <h3 style='margin-top: 0; color: #4F46E5;'>{curso.Titulo}</h3>
                            <p style='margin: 5px 0; font-size: 14px; color: #6b7280;'>Instructor: {curso.Asesor.Usuario.NombreCompleto}</p>
                            <hr style='border: 0; border-top: 1px solid #e5e7eb; margin: 15px 0;'>
                            <div style='display: flex; justify-content: space-between;'>
                                <span>Total Pagado:</span>
                                <strong>${curso.Costo:F2} MXN</strong>
                            </div>
                            <div style='display: flex; justify-content: space-between; margin-top: 5px;'>
                                <span>Método:</span>
                                <span>{metodoPago}</span>
                            </div>
                        </div>

                        <p>Ya puedes acceder al contenido completo desde tu panel de estudiante.</p>
                        <a href='{linkPerfil}' style='display: inline-block; background-color: #4F46E5; color: white; padding: 12px 24px; text-decoration: none; border-radius: 6px; font-weight: bold; margin-top: 10px;'>Ir a mi Aprendizaje</a>
                    </div>
                    <div style='background-color: #f3f4f6; padding: 15px; text-align: center; font-size: 12px; color: #6b7280;'>
                        &copy; 2025 Lumina Learning. Todos los derechos reservados.
                    </div>
                </div>
            ";

            await _emailService.SendEmailAsync(email, asunto, mensaje);
        }

        // ... (El resto de métodos GetMisCursosAsync se queda igual)
        public async Task<IEnumerable<CursoPublicDto>> GetMisCursosAsync(int estudianteId)
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

        public async Task<IEnumerable<HistorialPagoDto>> GetHistorialPagosAsync(int estudianteId)
        {
            // 1. Obtener pagos de Postgres
            var pagos = await _analyticsDb.HistorialDePagos
                .Where(p => p.EstudianteId == estudianteId)
                .OrderByDescending(p => p.FechaPago)
                .ToListAsync();

            if (!pagos.Any()) return new List<HistorialPagoDto>();

            // 2. Obtener nombres de cursos de SQL Server
            var cursoIds = pagos.Select(p => p.CursoId).Distinct().ToList();
            var cursosInfo = await _appDb.Cursos
                .Where(c => cursoIds.Contains(c.CursoId))
                .ToDictionaryAsync(c => c.CursoId, c => c.Titulo);

            // 3. Mapear
            return pagos.Select(p => new HistorialPagoDto
            {
                PagoId = p.PagoId,
                NombreCurso = cursosInfo.ContainsKey(p.CursoId) ? cursosInfo[p.CursoId] : "Curso eliminado",
                Monto = p.Monto,
                MetodoPago = p.MetodoPago,
                FechaPago = p.FechaPago
            });
        }
    }
}