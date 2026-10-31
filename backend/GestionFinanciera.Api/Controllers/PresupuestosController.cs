using GestionFinancieraPersonal.Models;
using GestionFinancieraPersonal.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionFinancieraPersonal.Controllers
{
    [ApiController]
    [Authorize(Roles = "Usuario")]
    [Route("api/presupuestos")]
    public class PresupuestosController : ControllerBase
    {
        private readonly PresupuestoService _presupuestos;
        private readonly MovimientoService _movimientos;

        public PresupuestosController(PresupuestoService presupuestos, MovimientoService movimientos)
        {
            _presupuestos = presupuestos;
            _movimientos = movimientos;
        }

        // GET /api/presupuestos?idCategoria=3   (el parámetro solo marca la opción seleccionada)
        [HttpGet]
        public async Task<ActionResult<PresupuestoViewModel>> Obtener(int? idCategoria)
        {
            var idUsuario = User.ObtenerIdUsuario();
            var periodo = FormatoFechas.MesAnio(DateTime.Today);

            return new PresupuestoViewModel
            {
                Periodo = char.ToUpper(periodo[0]) + periodo.Substring(1),
                Presupuestos = await _presupuestos.ObtenerDelMesAsync(idUsuario),
                Categorias = await _movimientos.ObtenerCategoriasAsync(idUsuario, "Gasto", idCategoria)
            };
        }

        // POST /api/presupuestos
        [HttpPost]
        public async Task<IActionResult> Guardar(PresupuestoFormViewModel formulario) =>
            this.Respuesta(await _presupuestos.GuardarAsync(User.ObtenerIdUsuario(), formulario));

        // DELETE /api/presupuestos/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Eliminar(int id) =>
            this.Respuesta(await _presupuestos.EliminarAsync(User.ObtenerIdUsuario(), id));
    }
}