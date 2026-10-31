using GestionFinancieraPersonal.Areas.Admin.Models;
using GestionFinancieraPersonal.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionFinancieraPersonal.Controllers.Admin
{
    [ApiController]
    [Authorize(Roles = "Administrador")]
    [Route("api/admin/configuracion")]
    public class AdminConfiguracionController : ControllerBase
    {
        private readonly ConfiguracionService _configuracion;

        public AdminConfiguracionController(ConfiguracionService configuracion)
        {
            _configuracion = configuracion;
        }

        // GET /api/admin/configuracion
        [HttpGet]
        public async Task<ActionResult<ConfiguracionSistema>> Obtener()
        {
            var actual = await _configuracion.ObtenerAsync();
            return new ConfiguracionSistema
            {
                UmbralPresupuesto = actual.UmbralPresupuesto,
                DiasAvisoDeuda = actual.DiasAvisoDeuda,
                IntentosPermitidos = actual.IntentosPermitidos
            };
        }

        // PUT /api/admin/configuracion
        [HttpPut]
        public async Task<IActionResult> Guardar(ConfiguracionSistema modelo) =>
            this.Respuesta(await _configuracion.GuardarAjustesAsync(
                modelo.UmbralPresupuesto!.Value,
                modelo.DiasAvisoDeuda!.Value,
                modelo.IntentosPermitidos!.Value));
    }
}