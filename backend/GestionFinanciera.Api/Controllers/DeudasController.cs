using GestionFinancieraPersonal.Models;
using GestionFinancieraPersonal.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GestionFinancieraPersonal.Controllers
{
    [ApiController]
    [Authorize(Roles = "Usuario")]
    [Route("api/deudas")]
    public class DeudasController : ControllerBase
    {
        private readonly DeudaService _deudas;
        private readonly MovimientoService _movimientos;

        public DeudasController(DeudaService deudas, MovimientoService movimientos)
        {
            _deudas = deudas;
            _movimientos = movimientos;
        }

        // GET /api/deudas?idDeuda=1&idCuenta=2   (los parámetros solo marcan la opción seleccionada)
        [HttpGet]
        public async Task<ActionResult<DeudasViewModel>> Obtener(int? idDeuda, int? idCuenta)
        {
            var idUsuario = User.ObtenerIdUsuario();
            var deudas = await _deudas.ObtenerAsync(idUsuario);

            return new DeudasViewModel
            {
                Deudas = deudas,
                PagosRecientes = await _deudas.ObtenerPagosRecientesAsync(idUsuario),
                DeudaTotal = deudas.Where(d => d.Estado != "Pagada").Sum(d => d.MontoPendiente),
                Cuentas = await _movimientos.ObtenerCuentasAsync(idUsuario, idCuenta),
                DeudasPendientes = deudas
                    .Where(d => d.Estado != "Pagada")
                    .Select(d => new SelectListItem(
                        $"{d.Nombre} (pendiente {d.MontoPendiente:C})",
                        d.IdDeuda.ToString(),
                        d.IdDeuda == idDeuda))
                    .ToList()
            };
        }

        // POST /api/deudas
        [HttpPost]
        public async Task<IActionResult> Crear(DeudaFormViewModel nueva) =>
            this.Respuesta(await _deudas.CrearAsync(User.ObtenerIdUsuario(), nueva));

        // POST /api/deudas/pagos
        [HttpPost("pagos")]
        public async Task<IActionResult> Pagar(PagoDeudaFormViewModel pago) =>
            this.Respuesta(await _deudas.PagarAsync(User.ObtenerIdUsuario(), pago));

        // DELETE /api/deudas/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Eliminar(int id) =>
            this.Respuesta(await _deudas.EliminarAsync(User.ObtenerIdUsuario(), id));
    }
}