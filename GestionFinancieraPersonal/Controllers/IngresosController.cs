using Microsoft.AspNetCore.Mvc;
using GestionFinancieraPersonal.Datos;
using GestionFinancieraPersonal.Models;

namespace GestionFinancieraPersonal.Controllers
{
    public class IngresosController : Controller
    {
        // GET: /Ingresos
        public IActionResult Index()
        {
            return View(DatosSimulados.Ingresos.OrderByDescending(i => i.Fecha).ToList());
        }

        // POST: /Ingresos/Registrar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Registrar(Ingreso nuevoIngreso)
        {
            if (nuevoIngreso.Monto <= 0)
                ModelState.AddModelError("Monto", "El monto debe ser mayor a $0.00.");
            if (string.IsNullOrWhiteSpace(nuevoIngreso.Descripcion))
                ModelState.AddModelError("Descripcion", "Este campo es obligatorio.");

            if (!ModelState.IsValid)
                return View("Index", DatosSimulados.Ingresos.OrderByDescending(i => i.Fecha).ToList());

            nuevoIngreso.Id = DatosSimulados.Ingresos.Count == 0 ? 1 : DatosSimulados.Ingresos.Max(i => i.Id) + 1;
            nuevoIngreso.Fecha = DateTime.Now;
            DatosSimulados.Ingresos.Add(nuevoIngreso);
            return RedirectToAction("Index");
        }
    }
}
