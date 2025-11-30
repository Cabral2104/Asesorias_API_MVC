using Asesorias_API_MVC.Models;
using Microsoft.AspNetCore.Identity;

namespace Asesorias_API_MVC.Data
{
    public static class DbSeeder
    {
        // Método para "sembrar" los roles
        public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
        {
            // OJO: RoleManager<IdentityRole<int>>
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();

            string[] roleNames = { "Admin", "Asesor", "Estudiante" };

            foreach (var roleName in roleNames)
            {
                var roleExists = await roleManager.RoleExistsAsync(roleName);
                if (!roleExists)
                {
                    // IdentityRole<int> usa Id entero automáticamente
                    await roleManager.CreateAsync(new IdentityRole<int>(roleName));
                }
            }
        }
    }
}
