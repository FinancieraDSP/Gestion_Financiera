using GestionFinancieraPersonal.Models;
using GestionFinancieraPersonal.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GestionFinancieraPersonal.Controllers
{
    [ApiController]
    [Authorize(Roles = "Usuario")]
    [Route("api/ahorro")]
    public class AhorroController : ControllerBase
    {
        private readonly AhorroService _ahorro;
        private readonly MovimientoService _movimientos;

        public AhorroController(AhorroService ahorro, MovimientoService movimientos)
        {
            _ahorro = ahorro;
            _movimientos = movimientos;
        }

        // GET /api/ahorro?idMeta=1&idCuenta=2   (los parámetros solo marcan la opción seleccionada)
        [HttpGet]
        public async Task<ActionResult<AhorroViewModel>> Obtener(int? idMeta, int? idCuenta)
        {
            var idUsuario = User.ObtenerIdUsuario();
            var metas = await _ahorro.ObtenerMetasAsync(idUsuario);

            return new AhorroViewModel
            {
                Metas = metas,
                AhorroTotal = metas.Sum(m => m.MontoActual),
                Cuentas = await _movimientos.ObtenerCuentasAsync(idUsuario, idCuenta),
                MetasActivas = metas
                    .Where(m => m.Estado == "Activa")
                    .Select(m => new SelectListItem(
                        $"{m.Nombre} (faltan {(m.MontoObjetivo - m.MontoActual):C})",
                        m.IdMeta.ToString(),
                        m.IdMeta == idMeta))
                    .ToList()
            };
        }

        // POST /api/ahorro/metas
        [HttpPost("metas")]
        public async Task<IActionResult> CrearMeta(MetaFormViewModel nuevaMeta) =>
            this.Respuesta(await _ahorro.CrearMetaAsync(User.ObtenerIdUsuario(), nuevaMeta));

        // POST /api/ahorro/aportes
        [HttpPost("aportes")]
        public async Task<IActionResult> Aportar(AporteFormViewModel aporte) =>
            this.Respuesta(await _ahorro.AportarAsync(User.ObtenerIdUsuario(), aporte));

        // DELETE /api/ahorro/metas/5
        [HttpDelete("metas/{id:int}")]
        public async Task<IActionResult> EliminarMeta(int id)
        {
            var (resultado, devuelto, cuenta) = await _ahorro.EliminarMetaAsync(User.ObtenerIdUsuario(), id);
            if (!resultado.Exito) return this.Respuesta(resultado);

            return Ok(new EliminarMetaResponse { Devuelto = devuelto, Cuenta = cuenta });
        }
    }
}