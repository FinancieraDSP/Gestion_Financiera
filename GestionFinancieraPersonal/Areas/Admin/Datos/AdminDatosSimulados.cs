using GestionFinancieraPersonal.Areas.Admin.Models;

namespace GestionFinancieraPersonal.Areas.Admin.Datos
{
    // Único dato que sigue en memoria: la configuración del sistema (no tiene tabla en la BD).
    public static class AdminDatosSimulados
    {
        public static ConfiguracionSistema Configuracion = new();
    }
}