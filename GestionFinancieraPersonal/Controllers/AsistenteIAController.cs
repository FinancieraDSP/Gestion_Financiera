using Microsoft.AspNetCore.Mvc;
using GestionFinancieraPersonal.Datos;

namespace GestionFinancieraPersonal.Controllers
{
    public class AsistenteIAController : Controller
    {
        public IActionResult Index()
        {
            return View(DatosSimulados.Conversacion);
        }
    }
}
