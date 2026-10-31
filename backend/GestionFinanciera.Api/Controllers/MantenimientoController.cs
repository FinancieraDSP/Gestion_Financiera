using GestionFinancieraPersonal.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionFinancieraPersonal.Controllers
{
    [ApiController]
    [AllowAnonymous]
    [Route("api/mantenimiento")]
    public class MantenimientoController : ControllerBase
    {
        private readonly MantenimientoService _mantenimiento;

        public MantenimientoController(MantenimientoService mantenimiento)
        {
            _mantenimiento = mantenimiento;
        }

        // GET /api/mantenimiento/estado
        [HttpGet("estado")]
        public async Task<ActionResult<InfoMantenimiento>> Estado() =>
            await _mantenimiento.ObtenerAsync();
    }
}