using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GestionFinancieraPersonal.Servicios;

namespace GestionFinancieraPersonal.Controllers
{
    // Pantalla pública de mantenimiento (se muestra a anónimos y a usuarios conectados)
    [AllowAnonymous]
    public class MantenimientoController : Controller
    {
        private readonly MantenimientoService _mantenimiento;

        public MantenimientoController(MantenimientoService mantenimiento)
        {
            _mantenimiento = mantenimiento;
        }

        public async Task<IActionResult> Index()
        {
            var info = await _mantenimiento.ObtenerAsync();

            // Si ya no hay mantenimiento en curso, se vuelve a la aplicación
            if (info.Estado != EstadoMantenimiento.EnCurso)
                return RedirectToAction("Login", "Account");

            return View(info);
        }
    }
}
