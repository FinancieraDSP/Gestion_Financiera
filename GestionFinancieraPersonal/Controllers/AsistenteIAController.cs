using GestionFinancieraPersonal.Datos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionFinancieraPersonal.Controllers
{
    [Authorize(Roles = "Usuario")]
    public class AsistenteIAController : Controller
    {
        public IActionResult Index()
        {
            return View(DatosSimulados.Conversacion);
        }
    }
}
