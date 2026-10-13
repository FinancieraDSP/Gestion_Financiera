using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestionFinancieraPersonal.Areas.Admin.Models;
using GestionFinancieraPersonal.Datos;
using GestionFinancieraPersonal.Servicios;

namespace GestionFinancieraPersonal.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Administrador")]
    public class UsuariosController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UsuarioService _usuarios;

        public UsuariosController(ApplicationDbContext db, UsuarioService usuarios)
        {
            _db = db;
            _usuarios = usuarios;
        }

        // GET: /Admin/Usuarios
        public async Task<IActionResult> Index(string? busqueda)
        {
            var consulta = ConsultaUsuarios();
            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                var b = busqueda.Trim();
                consulta = consulta.Where(u => u.Nombre.Contains(b) || u.Correo.Contains(b));
            }
            return View(await consulta.OrderBy(u => u.Nombre).ToListAsync());
        }

        // GET: /Admin/Usuarios/Acciones/3
        public async Task<IActionResult> Acciones(int id)
        {
            var usuario = await ConsultaUsuarios().FirstOrDefaultAsync(u => u.Id == id);
            if (usuario == null) return NotFound();
            return View(usuario);
        }

        // GET: /Admin/Usuarios/Crear
        [HttpGet]
        public IActionResult Crear()
        {
            return View(new UsuarioCrearViewModel());
        }

        // POST: /Admin/Usuarios/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(UsuarioCrearViewModel modelo)
        {
            if (!ModelState.IsValid)
                return View(modelo);

            var (resultado, usuario) = await _usuarios.CrearUsuarioAsync(
                modelo.Nombre, modelo.Correo, modelo.Contrasena, modelo.Rol);

            if (!resultado.Exito)
            {
                ModelState.AddModelError(resultado.Campo, resultado.Error!);
                return View(modelo);
            }

            TempData["Exito"] = $"Usuario {usuario!.Correo} creado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Admin/Usuarios/Bloquear/3
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Bloquear(int id)
        {
            var resultado = await _usuarios.CambiarEstadoAsync(id, false);
            Notificar(resultado, "Cuenta bloqueada.");
            return RedirectToAction(nameof(Index));
        }

        // POST: /Admin/Usuarios/Desbloquear/3
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Desbloquear(int id)
        {
            var resultado = await _usuarios.CambiarEstadoAsync(id, true);
            Notificar(resultado, "Cuenta desbloqueada.");
            return RedirectToAction(nameof(Index));
        }

        // POST: /Admin/Usuarios/Restablecer/3
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restablecer(int id)
        {
            var (resultado, clave, correo) = await _usuarios.RestablecerContrasenaAsync(id);
            if (resultado.Exito)
                TempData["Exito"] = $"Contraseña temporal de {correo}: {clave}  (solo se muestra ahora, compártela con el usuario).";
            else
                TempData["Error"] = resultado.Error;
            return RedirectToAction(nameof(Index));
        }

        // POST: /Admin/Usuarios/Eliminar/3
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            var resultado = await _usuarios.EliminarUsuarioAsync(id);
            Notificar(resultado, "Cuenta eliminada junto con todos sus datos.");
            return RedirectToAction(nameof(Index));
        }

        // ---------- auxiliares ----------

        private IQueryable<UsuarioAdmin> ConsultaUsuarios() =>
            _db.Usuarios.AsNoTracking().Select(u => new UsuarioAdmin
            {
                Id = u.IdUsuario,
                Nombre = u.Nombre,
                Correo = u.Correo,
                FechaRegistro = u.FechaRegistro,
                Rol = u.Rol.Nombre,
                Activo = u.Estado,
                CategoriasCreadas = _db.Categorias.Count(c => c.IdUsuario == u.IdUsuario)
            });

        private void Notificar(ResultadoOperacion resultado, string mensajeExito)
        {
            if (resultado.Exito) TempData["Exito"] = mensajeExito;
            else TempData["Error"] = resultado.Error;
        }
    }
}