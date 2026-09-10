using Microsoft.AspNetCore.Mvc;
using GestionFinancieraPersonal.Datos;
using GestionFinancieraPersonal.Models;

namespace GestionFinancieraPersonal.Controllers
{
    public class AhorroController : Controller
    {
        // GET: /Ahorro
        public IActionResult Index()
        {
            ViewBag.AhorroTotal = DatosSimulados.AhorroTotal;
            return View(DatosSimulados.Metas);
        }

        // POST: /Ahorro/Registrar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Registrar(MetaAhorro nuevaMeta)
        {
            if (nuevaMeta.MontoObjetivo <= 0)
                ModelState.AddModelError("MontoObjetivo", "La meta debe ser mayor a $0.00.");
            if (string.IsNullOrWhiteSpace(nuevaMeta.Nombre))
                ModelState.AddModelError("Nombre", "Este campo es obligatorio.");

            if (!ModelState.IsValid)
            {
                ViewBag.AhorroTotal = DatosSimulados.AhorroTotal;
                return View("Index", DatosSimulados.Metas);
            }

            nuevaMeta.Id = DatosSimulados.Metas.Count == 0 ? 1 : DatosSimulados.Metas.Max(m => m.Id) + 1;
            nuevaMeta.MontoActual = 0;
            if (string.IsNullOrWhiteSpace(nuevaMeta.Icono)) nuevaMeta.Icono = "ti-target";
            DatosSimulados.Metas.Add(nuevaMeta);
            return RedirectToAction("Index");
        }
    }
}
