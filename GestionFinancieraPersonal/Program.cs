using System.Globalization;
using System.Security.Claims;
using GestionFinancieraPersonal.Datos;
using GestionFinancieraPersonal.Servicios;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;

// Cultura fija: decimales con punto y montos con $
var cultura = new CultureInfo("en-US");
CultureInfo.DefaultThreadCurrentCulture = cultura;
CultureInfo.DefaultThreadCurrentUICulture = cultura;

var builder = WebApplication.CreateBuilder(args);

// ---------- Logging: consola + archivo diario en /logs (se conservan 14 días) ----------
builder.Host.UseSerilog((contexto, configuracion) => configuracion
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(
        path: Path.Combine(builder.Environment.ContentRootPath, "logs", "finanzapp-.log"),
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 14,
        shared: true,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}"));

builder.Services.AddControllersWithViews();

// ---------- Base de datos (LocalDB) ----------
var cadenaConexion = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Falta la cadena de conexión 'DefaultConnection'.");
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(cadenaConexion));

// ---------- Servicios de la aplicación (capa de lógica de negocio) ----------
builder.Services.AddScoped<MovimientoService>();
builder.Services.AddScoped<UsuarioService>();
builder.Services.AddScoped<CategoriaService>();
builder.Services.AddScoped<PresupuestoService>();
builder.Services.AddScoped<NotificacionService>();
builder.Services.AddScoped<AhorroService>();
builder.Services.AddScoped<DeudaService>();

// ---------- Autenticación por cookie ----------
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccesoDenegado";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.Name = "FinanzApp.Auth";

        // Si el admin bloquea o elimina al usuario, su sesión deja de valer de inmediato
        options.Events.OnValidatePrincipal = async context =>
        {
            var idClaim = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var db = context.HttpContext.RequestServices.GetRequiredService<ApplicationDbContext>();

            var activo = int.TryParse(idClaim, out var idUsuario) &&
                         await db.Usuarios.AnyAsync(u => u.IdUsuario == idUsuario && u.Estado);

            if (!activo)
            {
                context.RejectPrincipal();
                await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

// ---------- Crear BD y datos iniciales al arrancar ----------
using (var scope = app.Services.CreateScope())
{
    try
    {
        InicializadorBD.CrearBaseSiNoExiste(cadenaConexion);
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

// ---------- Manejo global de errores ----------
// Para depurar con la página técnica de .NET, cambia false por true:
var mostrarDetalleErrores = false;
if (mostrarDetalleErrores)
    app.UseDeveloperExceptionPage();
else
    app.UseExceptionHandler("/Home/Error");   // página amigable; el detalle queda en el log

app.UseStatusCodePagesWithReExecute("/Home/Estado", "?codigo={0}"); // 404, 403, etc.

if (!app.Environment.IsDevelopment())
    app.UseHsts();

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();   // primero: ¿quién eres?
app.UseAuthorization();    // después: ¿qué puedes hacer?

app.MapStaticAssets();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}")
    .WithStaticAssets();

app.Run();