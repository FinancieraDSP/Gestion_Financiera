using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using GestionFinancieraPersonal.Models;

namespace GestionFinancieraPersonal.Controllers
{
    public class HomeController : Controller
    {
        // Errores no controlados (500). El detalle técnico ya lo registró el middleware en el log.
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // Códigos de estado: 404, 403, etc.
        public IActionResult Estado(int codigo)
        {
            var (titulo, mensaje) = codigo switch
            {
                404 => ("Página no encontrada", "La dirección que buscas no existe o fue movida."),
                403 => ("Sin permiso", "No tienes permiso para ver esta sección."),
                _ => ("Algo salió mal", "No pudimos completar tu solicitud. Intenta de nuevo.")
            };

            ViewBag.Codigo = codigo;
            ViewBag.Titulo = titulo;
            ViewBag.Mensaje = mensaje;
            return View();
        }
    }
}