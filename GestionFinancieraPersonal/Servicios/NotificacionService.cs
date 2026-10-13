using Microsoft.EntityFrameworkCore;
using GestionFinancieraPersonal.Datos;
using GestionFinancieraPersonal.Entidades;

namespace GestionFinancieraPersonal.Servicios
{
    public class NotificacionService
    {
        private readonly ApplicationDbContext _db;

        public NotificacionService(ApplicationDbContext db)
        {
            _db = db;
        }

        public Task<int> ContarNoLeidasAsync(int idUsuario) =>
            _db.Notificaciones.CountAsync(n => n.IdUsuario == idUsuario && !n.Leida);

        public Task<List<Notificacion>> ObtenerAsync(int idUsuario, int max = 50) =>
            _db.Notificaciones.AsNoTracking()
                .Where(n => n.IdUsuario == idUsuario)
                .OrderByDescending(n => n.Fecha).ThenByDescending(n => n.IdNotificacion)
                .Take(max)
                .ToListAsync();

        public async Task MarcarTodasLeidasAsync(int idUsuario)
        {
            await _db.Notificaciones
                .Where(n => n.IdUsuario == idUsuario && !n.Leida)
                .ExecuteUpdateAsync(s => s.SetProperty(n => n.Leida, true));
        }
    }
}
