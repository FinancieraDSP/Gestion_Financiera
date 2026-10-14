using GestionFinancieraPersonal.Datos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionFinancieraPersonal.Controllers
{
    [Authorize(Roles = "Usuario")]
    public class AsistenteIAController : Controller
    {
        public IActionResult Recomendaciones()
        {
            var recomendaciones = DatosSimulados.Recomendaciones
                .OrderByDescending(r => r.FechaGeneracion)
                .Take(5)
                .ToList();
            return View(recomendaciones);
        }

        public IActionResult Chat()
        {
            return View(DatosSimulados.Conversacion);
        }
    }
}