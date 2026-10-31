using GestionFinancieraPersonal.Areas.Admin.Models;
using GestionFinancieraPersonal.Datos;
using GestionFinancieraPersonal.Models;
using GestionFinancieraPersonal.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestionFinancieraPersonal.Controllers.Admin
{
    [ApiController]
    [Authorize(Roles = "Administrador")]
    [Route("api/admin/usuarios")]
    public class AdminUsuariosController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly UsuarioService _usuarios;

        public AdminUsuariosController(ApplicationDbContext db, UsuarioService usuarios)
        {
            _db = db;
            _usuarios = usuarios;
        }

        // GET /api/admin/usuarios?busqueda=ana
        [HttpGet]
        public async Task<ActionResult<List<UsuarioAdmin>>> Listar(string? busqueda)
        {
            var consulta = ConsultaUsuarios();
            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                var b = busqueda.Trim();
                consulta = consulta.Where(u => u.Nombre.Contains(b) || u.Correo.Contains(b));
            }
            return await consulta.OrderBy(u => u.Nombre).ToListAsync();
        }

        // GET /api/admin/usuarios/3
        [HttpGet("{id:int}")]
        public async Task<ActionResult<UsuarioAdmin>> Obtener(int id)
        {
            var usuario = await ConsultaUsuarios().FirstOrDefaultAsync(u => u.Id == id);
            if (usuario == null) return NotFound();
            return usuario;
        }

        // POST /api/admin/usuarios
        [HttpPost]
        public async Task<IActionResult> Crear(UsuarioCrearViewModel modelo)
        {
            var (resultado, _) = await _usuarios.CrearUsuarioAsync(
                modelo.Nombre, modelo.Correo, modelo.Contrasena, modelo.Rol);
            return this.Respuesta(resultado);
        }

        // POST /api/admin/usuarios/3/bloquear
        [HttpPost("{id:int}/bloquear")]
        public async Task<IActionResult> Bloquear(int id) =>
            this.Respuesta(await _usuarios.CambiarEstadoAsync(id, false));

        // POST /api/admin/usuarios/3/desbloquear
        [HttpPost("{id:int}/desbloquear")]
        public async Task<IActionResult> Desbloquear(int id) =>
            this.Respuesta(await _usuarios.CambiarEstadoAsync(id, true));

        // DELETE /api/admin/usuarios/3
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Eliminar(int id) =>
            this.Respuesta(await _usuarios.EliminarUsuarioAsync(id));

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
    }
}