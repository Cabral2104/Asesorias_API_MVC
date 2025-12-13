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

// --- 1. Configurar CORS (Permitir conexiones desde React) ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()    // Permitir cualquier origen (tu frontend)
                   .AllowAnyMethod()    // Permitir GET, POST, PUT, DELETE
                   .AllowAnyHeader();   // Permitir cualquier cabecera (Tokens, etc.)
        });
});

// --- 2. Configurar Bases de Datos ---

// SQL Server (Datos principales)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        // AUMENTAR TIMEOUT A 3 MINUTOS (180 segundos)
        sqlOptions.CommandTimeout(180);
        // Reintentar si falla la conexión momentáneamente
        sqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
    }));

// B. PostgreSQL (Analíticas)
var analyticsConnectionString = builder.Configuration.GetConnectionString("AnalyticsConnection");
builder.Services.AddDbContext<AnalyticsDbContext>(options =>
    options.UseNpgsql(analyticsConnectionString, npgsqlOptions =>
    {
        // AUMENTAR TIMEOUT A 3 MINUTOS TAMBIÉN AQUÍ
        npgsqlOptions.CommandTimeout(180);
    }));
// --- 3. Configurar Identity (Usuarios y Roles) ---
builder.Services.AddIdentity<Usuario, IdentityRole<int>>(options =>
{
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Configurar vida de los tokens de recuperación de contraseña (2 horas)
builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
{
    options.TokenLifespan = TimeSpan.FromHours(2);
});

// --- 4. Configurar Autenticación JWT ---
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

// --- 5. Configurar Controladores y JSON ---
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Evita errores de referencia circular al serializar relaciones
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

// --- 6. Configurar Swagger (Documentación API) ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Definir la seguridad (Bearer Token) en Swagger
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Introduce tu token JWT aquí."
    });

    // Aplicar seguridad a todos los endpoints en Swagger UI
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

// --- 7. Inyección de Dependencias (Servicios) ---
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAsesorService, AsesorService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<ICursoService, CursoService>();
builder.Services.AddScoped<ILeccionService, LeccionService>();
builder.Services.AddScoped<IEstudianteService, EstudianteService>();
builder.Services.AddScoped<ICalificacionService, CalificacionService>();
builder.Services.AddScoped<ISolicitudService, SolicitudService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IProgresoService, ProgresoService>();

// =========================================================
// CONSTRUCCIÓN DE LA APLICACIÓN
// =========================================================
var app = builder.Build();

// --- 8. Seeding de Datos (Crear roles si no existen) ---
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

// --- 9. Pipeline de Peticiones HTTP ---

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Activar CORS (¡Importante para que React se conecte!)
app.UseCors("AllowAll");

// Activar Autenticación y Autorización
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();