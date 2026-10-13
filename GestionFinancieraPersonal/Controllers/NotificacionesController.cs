using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GestionFinancieraPersonal.Servicios;

//AL ABRIR LA PAGINA SE MUESTRAN LAS NO LEIDAS RESALTADAS Y LUEGO SE MARCAN COMO LEIDAS
namespace GestionFinancieraPersonal.Controllers
{

    [Authorize(Roles = "Usuario")]
    public class NotificacionesController : Controller
    {

        private readonly NotificacionService _servicio;

        public NotificacionesController(NotificacionService servicio)
        {
            _servicio = servicio;
        }

        // GET: /Notificaciones
        public async Task<IActionResult> Index()
        {
            var idUsuario = User.ObtenerIdUsuario();
            var lista = await _servicio.ObtenerAsync(idUsuario); // foto con el estado "leída" de antes
            await _servicio.MarcarTodasLeidasAsync(idUsuario);
            return View(lista);
        }
    }
}