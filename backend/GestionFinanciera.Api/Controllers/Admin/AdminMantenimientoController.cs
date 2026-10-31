using GestionFinancieraPersonal.Models;
using GestionFinancieraPersonal.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionFinancieraPersonal.Controllers.Admin
{
    [ApiController]
    [Authorize(Roles = "Administrador")]
    [Route("api/admin/mantenimiento")]
    public class AdminMantenimientoController : ControllerBase
    {
        private readonly MantenimientoService _mantenimiento;

        public AdminMantenimientoController(MantenimientoService mantenimiento)
        {
            _mantenimiento = mantenimiento;
        }

        // POST /api/admin/mantenimiento
        [HttpPost]
        public async Task<IActionResult> Programar(ProgramarMantenimientoRequest solicitud) =>
            this.Respuesta(await _mantenimiento.GuardarAsync(solicitud.Inicio, solicitud.Fin));

        // POST /api/admin/mantenimiento/cancelar
        [HttpPost("cancelar")]
        public async Task<IActionResult> Cancelar() =>
            this.Respuesta(await _mantenimiento.CancelarAsync());

        // POST /api/admin/mantenimiento/finalizar
        [HttpPost("finalizar")]
        public async Task<IActionResult> Finalizar() =>
            this.Respuesta(await _mantenimiento.FinalizarAsync());
    }
}