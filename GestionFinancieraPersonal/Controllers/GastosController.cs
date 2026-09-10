using Microsoft.AspNetCore.Mvc;
using GestionFinancieraPersonal.Datos;
using GestionFinancieraPersonal.Models;

namespace GestionFinancieraPersonal.Controllers
{
    public class GastosController : Controller
    {
        // GET: /Gastos
        public IActionResult Index()
        {
            return View(DatosSimulados.Gastos.OrderByDescending(g => g.Fecha).ToList());
        }

        // POST: /Gastos/Registrar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Registrar(Gasto nuevoGasto)
        {
            if (nuevoGasto.Monto <= 0)
                ModelState.AddModelError("Monto", "El monto debe ser mayor a $0.00.");
            if (string.IsNullOrWhiteSpace(nuevoGasto.Descripcion))
                ModelState.AddModelError("Descripcion", "Este campo es obligatorio.");

            if (!ModelState.IsValid)
                return View("Index", DatosSimulados.Gastos.OrderByDescending(g => g.Fecha).ToList());

            nuevoGasto.Id = DatosSimulados.Gastos.Count == 0 ? 1 : DatosSimulados.Gastos.Max(g => g.Id) + 1;
            nuevoGasto.Fecha = DateTime.Now;
            DatosSimulados.Gastos.Add(nuevoGasto);
            return RedirectToAction("Index");
        }
    }
}
