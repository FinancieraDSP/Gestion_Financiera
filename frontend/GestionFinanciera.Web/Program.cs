using Azure.Storage.Blobs;
using GestionFinancieraPersonal.Filtros;
using GestionFinancieraPersonal.Servicios;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Serilog;
using Serilog.Events;
using System.Globalization;

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
        path: Path.Combine(builder.Environment.ContentRootPath, "logs", "finanzapp-web-.log"),
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 14,
        shared: true,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}"));

builder.Services.AddMemoryCache();
builder.Services.AddControllersWithViews(o =>
{
    o.Filters.Add<SesionExpiradaFilter>();
    o.Filters.Add<MantenimientoFilter>();
});

// ---------- Cliente de la API ----------
builder.Services.AddHttpContextAccessor();
// ---------- Llaves de sesión (Data Protection) ----------
// En Azure se guardan en Blob Storage para que sobrevivan a reinicios y despliegues.
// Sin esta variable (por ejemplo en tu PC con Docker), se usa el volumen local como hasta ahora.
var blobKeys = builder.Configuration["DataProtection:BlobConnectionString"];
if (!string.IsNullOrWhiteSpace(blobKeys))
{
    builder.Services.AddDataProtection()
        .SetApplicationName("FinanzApp.Web")
        .PersistKeysToAzureBlobStorage(new BlobClient(blobKeys, "dataprotection", "keys.xml"));
}
builder.Services.AddHttpClient<ApiClient>(cliente =>
{
    var urlApi = builder.Configuration["Api:BaseUrl"]
        ?? throw new InvalidOperationException("Falta 'Api:BaseUrl'.");
    cliente.BaseAddress = new Uri(urlApi);
    cliente.Timeout = TimeSpan.FromSeconds(90); // la 1.ª petición tras una pausa de Azure SQL puede tardar
});

// ---------- Autenticación por cookie (guarda el token JWT de la API dentro de la cookie cifrada) ----------
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccesoDenegado";
        options.SlidingExpiration = false;   // la cookie vence junto con el token
        options.Cookie.HttpOnly = true;
        options.Cookie.Name = "FinanzApp.Auth";
    });
builder.Services.AddAuthorization();

var app = builder.Build();

// ---------- Manejo global de errores ----------
var mostrarDetalleErrores = false;
if (mostrarDetalleErrores)
    app.UseDeveloperExceptionPage();
else
    app.UseExceptionHandler("/Home/Error");

app.UseStatusCodePagesWithReExecute("/Home/Estado", "?codigo={0}");

if (!app.Environment.IsDevelopment())
    app.UseHsts();

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

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