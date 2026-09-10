using Microsoft.AspNetCore.Mvc;
using GestionFinancieraPersonal.Datos;

namespace GestionFinancieraPersonal.Controllers
{
    public class DeudasController : Controller
    {
        public IActionResult Index()
        {
            ViewBag.DeudaTotal = DatosSimulados.Deudas.Sum(d => d.MontoPendiente);
            return View(DatosSimulados.Deudas.OrderBy(d => d.Vencimiento).ToList());
        }
    }
}
