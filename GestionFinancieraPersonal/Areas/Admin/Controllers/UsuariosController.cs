using Microsoft.AspNetCore.Mvc;
using GestionFinancieraPersonal.Areas.Admin.Datos;

namespace GestionFinancieraPersonal.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class UsuariosController : Controller
    {
        // GET: /Admin/Usuarios
        public IActionResult Index(string? busqueda)
        {
            var lista = AdminDatosSimulados.Usuarios.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                lista = lista.Where(u =>
                    u.Nombre.Contains(busqueda, StringComparison.OrdinalIgnoreCase) ||
                    u.Correo.Contains(busqueda, StringComparison.OrdinalIgnoreCase));
            }
            return View(lista.ToList());
        }

        // GET: /Admin/Usuarios/Acciones/3
        public IActionResult Acciones(int id)
        {
            var usuario = AdminDatosSimulados.Usuarios.FirstOrDefault(u => u.Id == id);
            if (usuario == null) return NotFound();
            return View(usuario);
        }

        // POST: /Admin/Usuarios/Bloquear/3
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Bloquear(int id)
        {
            var usuario = AdminDatosSimulados.Usuarios.FirstOrDefault(u => u.Id == id);
            if (usuario != null) usuario.Activo = false;
            return RedirectToAction("Index");
        }

        // POST: /Admin/Usuarios/Desbloquear/3
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Desbloquear(int id)
        {
            var usuario = AdminDatosSimulados.Usuarios.FirstOrDefault(u => u.Id == id);
            if (usuario != null) { usuario.Activo = true; usuario.MotivoSuspension = null; }
            return RedirectToAction("Index");
        }
    }
}
