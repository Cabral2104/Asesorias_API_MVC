// Ruta: Asesorias.API/Program.cs
using Asesorias_API_MVC.Data;
using Asesorias_API_MVC.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// --- 1. CONFIGURAR SERVICIOS ---

// Obtener la cadena de conexión
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Configurar el DbContext (La conexión a la BDD)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Configurar Identity (Usuarios, Roles, Login)
builder.Services.AddIdentity<Usuario, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders(); // Permite generar tokens para resetear contraseña

// Configurar Controladores y Swagger
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Ignorar ciclos de referencia al consultar datos (Ej: Curso -> Asesor -> Cursos)
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// --- 2. CONSTRUIR LA APP ---
var app = builder.Build();

// Configurar el pipeline de HTTP (el orden importa)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// app.UseAuthentication(); // Habilitaremos esto más adelante
app.UseAuthorization();

app.MapControllers();

app.Run();