using GestionFinancieraPersonal.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionFinancieraPersonal.Controllers
{
    // Pantalla pública de mantenimiento (se muestra a anónimos y a usuarios conectados)
    [AllowAnonymous]
    public class MantenimientoController : Controller
    {
        private readonly ApiClient _api;

        public MantenimientoController(ApiClient api)
        {
            _api = api;
        }

        public async Task<IActionResult> Index()
        {
            var respuesta = await _api.GetAsync<InfoMantenimiento>("api/mantenimiento/estado");

            // Si ya no hay mantenimiento en curso, se vuelve a la aplicación
            if (!respuesta.Exito || respuesta.Datos == null || respuesta.Datos.Estado != EstadoMantenimiento.EnCurso)
                return RedirectToAction("Login", "Account");

            return View(respuesta.Datos);
        }
    }
}