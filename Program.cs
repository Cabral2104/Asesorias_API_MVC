
using Asesorias_API_MVC.Data;
using Asesorias_API_MVC.Models;
using Asesorias_API_MVC.Services.Implementations;
using Asesorias_API_MVC.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// --- 1. CONFIGURAR SERVICIOS ---

// --- NUEVO: Configurar CORS ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()    // Permitir cualquier origen
                   .AllowAnyMethod()    // Permitir cualquier método (GET, POST, etc.)
                   .AllowAnyHeader();   // Permitir cualquier cabecera
        });
});

// Obtener la cadena de conexión con SQLServer
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Configurar el DbContext de SQLServer
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Obtener la cadena de conexión (PostgreSQL)
var analyticsConnectionString = builder.Configuration.GetConnectionString("AnalyticsConnection");

// Configurar el 2do DbContext (PostgreSQL)
builder.Services.AddDbContext<AnalyticsDbContext>(options =>
    options.UseNpgsql(analyticsConnectionString));

// Configurar Identity
builder.Services.AddIdentity<Usuario, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Configurar Autenticación JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // 1. Definir la seguridad (Bearer Token)
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Por favor, introduce tu token JWT con el prefijo 'Bearer ' en el campo."
    });

    // 2. Hacer que Swagger aplique este requisito a todos los endpoints
    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// --- ESTA ES LA LÍNEA DEL PASO 4 ---
// Registra nuestro "Chef" (AuthService) cada vez que alguien pida el "Contrato" (IAuthService)
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddScoped<IAsesorService, AsesorService>();

builder.Services.AddScoped<IAdminService, AdminService>();

builder.Services.AddScoped<ICursoService, CursoService>();

builder.Services.AddScoped<ILeccionService, LeccionService>();

builder.Services.AddScoped<IEstudianteService, EstudianteService>();

builder.Services.AddScoped<ICalificacionService, CalificacionService>();

// --- 2. CONSTRUIR LA APP ---
var app = builder.Build();

// Ejecutar el Seeder de Roles al inicio
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await DbSeeder.SeedRolesAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocurrió un error durante la siembra de datos");
    }
}

// Configurar el pipeline de HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// --- NUEVO: Usar la política CORS ---
app.UseCors("AllowAll");

app.UseStaticFiles();

// Habilitar Autenticación y Autorización
app.UseAuthentication();
app.UseAuthorization();


app.MapControllers();

app.Run();