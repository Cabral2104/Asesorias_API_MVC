using Asesorias_API_MVC.Models;
using Asesorias_API_MVC.Models.Dtos;
using Asesorias_API_MVC.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Asesorias_API_MVC.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;

        public AuthService(UserManager<Usuario> userManager, IConfiguration configuration, IEmailService emailService)
        {
            _userManager = userManager;
            _configuration = configuration;
            _emailService = emailService;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegistroUsuarioDto registerDto)
        {
            var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
            if (existingUser != null)
            {
                return new AuthResponseDto { IsSuccess = false, Message = "El correo electrónico ya está en uso." };
            }

            // --- CORRECCIÓN: Generar UserName único basado en el nombre ---
            // Ej: Juan -> Juan_a1b2
            var randomSuffix = Guid.NewGuid().ToString().Substring(0, 4);
            var uniqueUserName = $"{registerDto.UserName.Replace(" ", "")}_{randomSuffix}";

            var newUser = new Usuario
            {
                UserName = uniqueUserName, // Único para el sistema
                Email = registerDto.Email,
                NombreCompleto = registerDto.UserName, // El nombre real que se puede repetir
                SecurityStamp = Guid.NewGuid().ToString()
            };

            var result = await _userManager.CreateAsync(newUser, registerDto.Password);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return new AuthResponseDto { IsSuccess = false, Message = errors };
            }

            await _userManager.AddToRoleAsync(newUser, "Estudiante");

            var roles = await _userManager.GetRolesAsync(newUser);
            var token = GenerateJwtToken(newUser, roles);

            return new AuthResponseDto
            {
                IsSuccess = true,
                Message = "¡Cuenta creada exitosamente!",
                Email = newUser.Email,
                Token = token
            };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
        {
            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user == null || !user.IsActive)
            {
                return new AuthResponseDto { IsSuccess = false, Message = "Credenciales inválidas." };
            }

            var isPasswordValid = await _userManager.CheckPasswordAsync(user, loginDto.Password);
            if (!isPasswordValid)
            {
                return new AuthResponseDto { IsSuccess = false, Message = "Credenciales inválidas." };
            }

            user.LastLogin = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            var roles = await _userManager.GetRolesAsync(user);
            var token = GenerateJwtToken(user, roles);

            return new AuthResponseDto
            {
                IsSuccess = true,
                Message = "¡Inicio de sesión exitoso!",
                Email = user.Email,
                Token = token
            };
        }

        // --- ACTUALIZAR PERFIL ---
        public async Task<GenericResponseDto> UpdateProfileAsync(string userId, UpdateProfileDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return new GenericResponseDto { IsSuccess = false, Message = "Usuario no encontrado" };

            user.NombreCompleto = dto.NombreCompleto;
            user.PhoneNumber = dto.PhoneNumber;

            await _userManager.UpdateAsync(user);
            return new GenericResponseDto { IsSuccess = true, Message = "Perfil actualizado." };
        }

        // --- RECUPERAR CONTRASEÑA ---
        public async Task<GenericResponseDto> ForgotPasswordAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return new GenericResponseDto { IsSuccess = false, Message = "Si el correo existe, se ha enviado un enlace." };

            // Genera un token que dura 2 horas (según configuración en Program.cs)
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var message = $@"
                <div style='font-family: Arial, sans-serif; padding: 20px; border: 1px solid #ddd; border-radius: 10px;'>
                    <h2 style='color: #4F46E5;'>Recuperación de Contraseña</h2>
                    <p>Has solicitado restablecer tu contraseña en Lumina.</p>
                    <p>Copia el siguiente código de seguridad:</p>
                    <div style='background: #f3f4f6; padding: 15px; text-align: center; font-size: 24px; letter-spacing: 5px; font-weight: bold; border-radius: 5px;'>
                        {token}
                    </div>
                    <p style='margin-top: 20px; font-size: 12px; color: #666;'>Este código expirará en 2 horas.</p>
                </div>
            ";

            try
            {
                await _emailService.SendEmailAsync(email, "Código de Seguridad - Lumina", message);
                return new GenericResponseDto { IsSuccess = true, Message = "Correo enviado." };
            }
            catch
            {
                return new GenericResponseDto { IsSuccess = false, Message = "Error al enviar el correo." };
            }
        }

        public async Task<GenericResponseDto> ResetPasswordAsync(ResetPasswordDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return new GenericResponseDto { IsSuccess = false, Message = "Error al restablecer." };

            var result = await _userManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return new GenericResponseDto { IsSuccess = false, Message = errors };
            }

            return new GenericResponseDto { IsSuccess = true, Message = "Contraseña restablecida con éxito." };
        }

        private string GenerateJwtToken(Usuario user, IEnumerable<string> roles)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email ?? ""),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                // Priorizamos mostrar el NombreCompleto, si no el UserName
                new Claim("NombreCompleto", user.NombreCompleto ?? user.UserName)
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
