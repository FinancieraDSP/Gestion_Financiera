using Microsoft.EntityFrameworkCore;
using GestionFinancieraPersonal.Datos;
using GestionFinancieraPersonal.Entidades;

namespace GestionFinancieraPersonal.Servicios
{
    // Lee y guarda los ajustes del sistema (tabla de una sola fila).
    // Los demás servicios (alertas, deudas, login, mantenimiento) la consultan desde aquí.
    public class ConfiguracionService
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<ConfiguracionService> _logger;

        public ConfiguracionService(ApplicationDbContext db, ILogger<ConfiguracionService> logger)
        {
            _db = db;
            _logger = logger;
        }

        // Si por alguna razón la fila no existe, devuelve los valores por defecto (nunca null)
        public async Task<Configuracion> ObtenerAsync()
        {
            return await _db.Configuraciones.AsNoTracking().FirstOrDefaultAsync()
                   ?? new Configuracion();
        }

        public async Task<ResultadoOperacion> GuardarAjustesAsync(int umbralPresupuesto, int diasAvisoDeuda, int intentosPermitidos)
        {
            try
            {
                var config = await _db.Configuraciones.FirstOrDefaultAsync();
                if (config == null)
                {
                    config = new Configuracion();
                    _db.Configuraciones.Add(config);
                }

                config.UmbralPresupuesto = umbralPresupuesto;
                config.DiasAvisoDeuda = diasAvisoDeuda;
                config.IntentosPermitidos = intentosPermitidos;

                await _db.SaveChangesAsync(); // valida los rangos de la entidad antes de guardar
                _logger.LogInformation(
                    "Configuración actualizada: umbral {Umbral}%, aviso deudas {Dias} días, intentos {Intentos}",
                    umbralPresupuesto, diasAvisoDeuda, intentosPermitidos);
                return ResultadoOperacion.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar la configuración del sistema");
                return ResultadoOperacion.Fallo("No se pudo guardar la configuración. Intenta de nuevo.");
            }
        }
    }
}
