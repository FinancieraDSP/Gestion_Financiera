using Microsoft.EntityFrameworkCore;
using GestionFinancieraPersonal.Datos;
using GestionFinancieraPersonal.Entidades;

namespace GestionFinancieraPersonal.Servicios
{
    public class NotificacionService
    {
        private readonly ApplicationDbContext _db;
        private readonly DeudaService _deudas;
        private readonly MantenimientoService _mantenimiento;

        public NotificacionService(ApplicationDbContext db, DeudaService deudas, MantenimientoService mantenimiento)
        {
            _db = db;
            _deudas = deudas;
            _mantenimiento = mantenimiento;
        }

        // Notificaciones que el usuario ve (las eliminadas siguen en BD para que los generadores no las recreen)
        private IQueryable<Notificacion> Visibles(int idUsuario) =>
            _db.Notificaciones.Where(n => n.IdUsuario == idUsuario && !n.Eliminada);

        private async Task GenerarAvisosAsync(int idUsuario)
        {
            await _deudas.GenerarAvisosVencimientoAsync(idUsuario);
            await _mantenimiento.GenerarAvisoAsync(idUsuario);
        }

        public async Task<int> ContarNoLeidasAsync(int idUsuario)
        {
            await GenerarAvisosAsync(idUsuario);
            return await Visibles(idUsuario).CountAsync(n => !n.Leida);
        }

        public async Task<List<Notificacion>> ObtenerAsync(int idUsuario, int max = 50)
        {
            await GenerarAvisosAsync(idUsuario);
            return await Visibles(idUsuario).AsNoTracking()
                .OrderByDescending(n => n.Fecha).ThenByDescending(n => n.IdNotificacion)
                .Take(max)
                .ToListAsync();
        }

        public async Task<bool> MarcarLeidaAsync(int idUsuario, int idNotificacion) =>
            await Visibles(idUsuario)
                .Where(n => n.IdNotificacion == idNotificacion && !n.Leida)
                .ExecuteUpdateAsync(s => s.SetProperty(n => n.Leida, true)) > 0;

        public Task<int> MarcarTodasLeidasAsync(int idUsuario) =>
            Visibles(idUsuario).Where(n => !n.Leida)
                .ExecuteUpdateAsync(s => s.SetProperty(n => n.Leida, true));

        // Eliminar = ocultar (y dejar como leída para que no cuente en la campana)
        public async Task<bool> EliminarAsync(int idUsuario, int idNotificacion) =>
            await Visibles(idUsuario)
                .Where(n => n.IdNotificacion == idNotificacion)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(n => n.Eliminada, true)
                    .SetProperty(n => n.Leida, true)) > 0;

        public Task<int> EliminarTodasAsync(int idUsuario) =>
            Visibles(idUsuario).ExecuteUpdateAsync(s => s
                .SetProperty(n => n.Eliminada, true)
                .SetProperty(n => n.Leida, true));
    }
}