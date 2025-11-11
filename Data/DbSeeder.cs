using Asesorias_API_MVC.Models;
using Microsoft.AspNetCore.Identity;

namespace Asesorias_API_MVC.Data
{
    public static class DbSeeder
    {
        // Método para "sembrar" los roles
        public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
        {
            // 1. Obtener el servicio de manejo de roles
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // 2. Definir los roles que queremos
            string[] roleNames = { "Admin", "Asesor", "Estudiante" };

            // 3. Crear cada rol si NO existe
            foreach (var roleName in roleNames)
            {
                var roleExists = await roleManager.RoleExistsAsync(roleName);
                if (!roleExists)
                {
                    // Crear el rol
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
        }
    }
}
