using GestionFinancieraPersonal.Entidades;
using GestionFinancieraPersonal.Models;
using GestionFinancieraPersonal.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionFinancieraPersonal.Controllers
{
    [ApiController]
    [Authorize(Roles = "Usuario")]
    [Route("api/gastos")]
    public class GastosController : ControllerBase
    {
        private readonly MovimientoService _servicio;

        public GastosController(MovimientoService servicio)
        {
            _servicio = servicio;
        }

        // GET /api/gastos?idCuenta=1&idCategoria=2
        [HttpGet]
        public async Task<ActionResult<MovimientosViewModel<Gasto>>> Obtener(int? idCuenta, int? idCategoria)
        {
            var idUsuario = User.ObtenerIdUsuario();
            return new MovimientosViewModel<Gasto>
            {
                Items = await _servicio.ObtenerGastosAsync(idUsuario),
                Cuentas = await _servicio.ObtenerCuentasAsync(idUsuario, idCuenta),
                Categorias = await _servicio.ObtenerCategoriasAsync(idUsuario, "Gasto", idCategoria)
            };
        }

        // POST /api/gastos
        [HttpPost]
        public async Task<IActionResult> Registrar(MovimientoFormViewModel formulario) =>
            this.Respuesta(await _servicio.RegistrarGastoAsync(User.ObtenerIdUsuario(), formulario));

        // DELETE /api/gastos/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Eliminar(int id) =>
            this.Respuesta(await _servicio.EliminarGastoAsync(User.ObtenerIdUsuario(), id));
    }
}