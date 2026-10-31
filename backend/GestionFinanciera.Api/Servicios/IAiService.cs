using GestionFinancieraPersonal.Entidades;

namespace GestionFinancieraPersonal.Servicios
{
    public interface IAiService
    {
        Task<string> ClasificarGastoAsync(string descripcion, IEnumerable<string> categoriasDisponibles);
        Task<string> ChatAsync(int idUsuario, string mensajeUsuario, List<(string Rol, string Texto)>? historial = null);
        Task<List<Recomendacion>> GenerarRecomendacionesAsync(int idUsuario);
    }
}
