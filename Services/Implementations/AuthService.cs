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
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;

        public AuthService(UserManager<Usuario> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegistroUsuarioDto registerDto)
        {
            var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
            if (existingUser != null)
            {
                return new AuthResponseDto { IsSuccess = false, Message = "El correo electrónico ya está en uso." };
            }

            var newUser = new Usuario
            {
                UserName = registerDto.UserName,
                Email = registerDto.Email,
                SecurityStamp = Guid.NewGuid().ToString()
                // Ya no ponemos CreatedAt, el DbContext lo hará
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

        // --- MÉTODO LOGIN ACTUALIZADO ---
        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
        {
            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user == null)
            {
                return new AuthResponseDto { IsSuccess = false, Message = "Credenciales inválidas." };
            }

            // Ya no es necesario checar IsActive, el Filtro Global del DbContext
            // hará que FindByEmailAsync falle si el usuario está inactivo.
            // Pero una doble validación no hace daño (la dejamos por si acaso).
            if (!user.IsActive)
            {
                return new AuthResponseDto { IsSuccess = false, Message = "Esta cuenta ha sido desactivada." };
            }

            var isPasswordValid = await _userManager.CheckPasswordAsync(user, loginDto.Password);
            if (!isPasswordValid)
            {
                return new AuthResponseDto { IsSuccess = false, Message = "Credenciales inválidas." };
            }

            // --- ¡AQUÍ ESTÁ EL CAMBIO! ---
            // Actualizamos la fecha del último login
            user.LastLogin = DateTime.UtcNow;
            await _userManager.UpdateAsync(user); // Guardamos el cambio en la BDD
            // --- FIN DEL CAMBIO ---

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
        // --- FIN MÉTODO LOGIN ---

        private string GenerateJwtToken(Usuario user, IEnumerable<string> roles)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
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
