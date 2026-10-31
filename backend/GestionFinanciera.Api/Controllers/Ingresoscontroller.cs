using GestionFinancieraPersonal.Entidades;
using GestionFinancieraPersonal.Models;
using GestionFinancieraPersonal.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionFinancieraPersonal.Controllers
{
    [ApiController]
    [Authorize(Roles = "Usuario")]
    [Route("api/ingresos")]
    public class IngresosController : ControllerBase
    {
        private readonly MovimientoService _servicio;

        public IngresosController(MovimientoService servicio)
        {
            _servicio = servicio;
        }

        // GET /api/ingresos?idCuenta=1&idCategoria=2  (los parámetros solo marcan la opción seleccionada)
        [HttpGet]
        public async Task<ActionResult<MovimientosViewModel<Ingreso>>> Obtener(int? idCuenta, int? idCategoria)
        {
            var idUsuario = User.ObtenerIdUsuario();
            return new MovimientosViewModel<Ingreso>
            {
                Items = await _servicio.ObtenerIngresosAsync(idUsuario),
                Cuentas = await _servicio.ObtenerCuentasAsync(idUsuario, idCuenta),
                Categorias = await _servicio.ObtenerCategoriasAsync(idUsuario, "Ingreso", idCategoria)
            };
        }

        // POST /api/ingresos
        [HttpPost]
        public async Task<IActionResult> Registrar(MovimientoFormViewModel formulario) =>
            this.Respuesta(await _servicio.RegistrarIngresoAsync(User.ObtenerIdUsuario(), formulario));

        // DELETE /api/ingresos/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Eliminar(int id) =>
            this.Respuesta(await _servicio.EliminarIngresoAsync(User.ObtenerIdUsuario(), id));
    }
}