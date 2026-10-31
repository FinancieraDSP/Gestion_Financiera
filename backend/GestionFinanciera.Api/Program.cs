using System.Globalization;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using GestionFinancieraPersonal.Datos;
using GestionFinancieraPersonal.Models;
using GestionFinancieraPersonal.Servicios;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Events;

// Cultura fija: decimales con punto y montos con $
var cultura = new CultureInfo("en-US");
CultureInfo.DefaultThreadCurrentCulture = cultura;
CultureInfo.DefaultThreadCurrentUICulture = cultura;

var builder = WebApplication.CreateBuilder(args);

// ---------- Logging: consola + archivo diario en /logs ----------
builder.Host.UseSerilog((contexto, configuracion) => configuracion
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(
        path: Path.Combine(builder.Environment.ContentRootPath, "logs", "finanzapp-api-.log"),
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 14,
        shared: true,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}"));

builder.Services.AddControllers(o => o.Filters.Add<GestionFinancieraPersonal.Filtros.MantenimientoApiFilter>())
    .AddJsonOptions(o => o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

// ---------- Base de datos ----------
var cadenaConexion = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Falta la cadena de conexión 'DefaultConnection'.");
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(cadenaConexion));

// ---------- Correo ----------
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

// ---------- Servicios de la aplicación ----------
builder.Services.AddScoped<MovimientoService>();
builder.Services.AddScoped<UsuarioService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<CategoriaService>();
builder.Services.AddScoped<PresupuestoService>();
builder.Services.AddScoped<NotificacionService>();
builder.Services.AddScoped<AhorroService>();
builder.Services.AddScoped<DeudaService>();
builder.Services.AddScoped<ConfiguracionService>();
builder.Services.AddScoped<MantenimientoService>();
builder.Services.AddScoped<TokenService>();

// ---------- Autenticación JWT ----------
var jwt = builder.Configuration.GetSection("Jwt");
var claveJwt = jwt["Key"] ?? throw new InvalidOperationException("Falta 'Jwt:Key' (user-secrets o variable de entorno).");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.MapInboundClaims = false; // conservamos los nombres de claim tal como los emitimos
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwt["Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(claveJwt)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),
            NameClaimType = ClaimTypes.Name,
            RoleClaimType = ClaimTypes.Role
        };

        // Si el admin bloquea o elimina al usuario, su token deja de valer de inmediato
        o.Events = new JwtBearerEvents
        {
            OnTokenValidated = async contexto =>
            {
                var idClaim = contexto.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var db = contexto.HttpContext.RequestServices.GetRequiredService<ApplicationDbContext>();

                var activo = int.TryParse(idClaim, out var idUsuario) &&
                             await db.Usuarios.AnyAsync(u => u.IdUsuario == idUsuario && u.Estado);

                if (!activo) contexto.Fail("Usuario inactivo o inexistente.");
            }
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

// ---------- Crear BD y datos iniciales al arrancar (igual que antes) ----------
using (var scope = app.Services.CreateScope())
{
    try
    {
        InicializadorBD.CrearBaseSiNoExiste(cadenaConexion);
        ActualizadorEsquema.Aplicar(cadenaConexion);
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        InicializadorBD.SembrarDatos(db);
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogCritical(ex, "No se pudo inicializar la base de datos.");
        throw;
    }
}

// ---------- Errores no controlados: JSON limpio (el detalle queda en el log) ----------
app.UseExceptionHandler(errores => errores.Run(async contexto =>
{
    contexto.Response.StatusCode = StatusCodes.Status500InternalServerError;
    await contexto.Response.WriteAsJsonAsync(new ErrorApi { Mensaje = "Error interno del servidor." });
}));

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new { estado = "ok" })); // lo usaremos en Docker y Azure
app.MapControllers();

app.Run();