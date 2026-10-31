using GestionFinancieraPersonal.Entidades;
using GestionFinancieraPersonal.Models;
using GestionFinancieraPersonal.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionFinancieraPersonal.Controllers
{
    [ApiController]
    [Authorize(Roles = "Usuario")]
    [Route("api/categorias")]
    public class CategoriasController : ControllerBase
    {
        private readonly CategoriaService _servicio;

        public CategoriasController(CategoriaService servicio)
        {
            _servicio = servicio;
        }

        // GET /api/categorias
        [HttpGet]
        public async Task<ActionResult<MisCategoriasViewModel>> Obtener() =>
            new MisCategoriasViewModel
            {
                Categorias = await _servicio.ObtenerVisiblesAsync(User.ObtenerIdUsuario())
            };

        // GET /api/categorias/5   (solo categorías propias)
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Categoria>> ObtenerPropia(int id)
        {
            var categoria = await _servicio.ObtenerPropiaAsync(User.ObtenerIdUsuario(), id);
            if (categoria == null) return NotFound();
            return categoria;
        }

        // POST /api/categorias
        [HttpPost]
        public async Task<IActionResult> Crear(CategoriaUsuarioFormViewModel formulario) =>
            this.Respuesta(await _servicio.CrearAsync(User.ObtenerIdUsuario(), formulario));

        // PUT /api/categorias/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Editar(int id, CategoriaUsuarioFormViewModel formulario) =>
            this.Respuesta(await _servicio.EditarAsync(
                User.ObtenerIdUsuario(), id, formulario.Nombre, formulario.Descripcion));

        // DELETE /api/categorias/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var (resultado, desactivada) = await _servicio.EliminarAsync(User.ObtenerIdUsuario(), id);
            if (!resultado.Exito) return this.Respuesta(resultado);

            return Ok(new EliminarCategoriaResponse { Desactivada = desactivada });
        }

        // POST /api/categorias/5/reactivar
        [HttpPost("{id:int}/reactivar")]
        public async Task<IActionResult> Reactivar(int id) =>
            this.Respuesta(await _servicio.ReactivarAsync(User.ObtenerIdUsuario(), id));
    }
}