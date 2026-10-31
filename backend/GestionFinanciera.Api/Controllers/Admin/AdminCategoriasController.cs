using GestionFinancieraPersonal.Areas.Admin.Models;
using GestionFinancieraPersonal.Datos;
using GestionFinancieraPersonal.Entidades;
using GestionFinancieraPersonal.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestionFinancieraPersonal.Controllers.Admin
{
    [ApiController]
    [Authorize(Roles = "Administrador")]
    [Route("api/admin/categorias")]
    public class AdminCategoriasController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public AdminCategoriasController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET /api/admin/categorias/5   (solo categorías generales)
        [HttpGet("{id:int}")]
        public async Task<ActionResult<CategoriaFormViewModel>> Obtener(int id)
        {
            var categoria = await _db.Categorias.AsNoTracking()
                .FirstOrDefaultAsync(c => c.IdCategoria == id && c.IdUsuario == null);
            if (categoria == null) return NotFound();

            return new CategoriaFormViewModel
            {
                Id = categoria.IdCategoria,
                Nombre = categoria.Nombre,
                Tipo = categoria.Tipo,
                Descripcion = categoria.Descripcion
            };
        }

        // POST /api/admin/categorias
        [HttpPost]
        public async Task<IActionResult> Crear(CategoriaFormViewModel modelo)
        {
            var nombre = modelo.Nombre.Trim();
            if (await _db.Categorias.AnyAsync(c => c.IdUsuario == null && c.Tipo == modelo.Tipo && c.Nombre == nombre))
                return BadRequest(new ErrorApi { Mensaje = "Ya existe una categoría general con ese nombre.", Campo = "Nombre" });

            _db.Categorias.Add(new Categoria
            {
                IdUsuario = null, // null = categoría general, visible para todos
                Nombre = nombre,
                Tipo = modelo.Tipo,
                Descripcion = string.IsNullOrWhiteSpace(modelo.Descripcion) ? null : modelo.Descripcion.Trim()
            });
            await _db.SaveChangesAsync();
            return Ok();
        }

        // PUT /api/admin/categorias/5   (el tipo no se puede cambiar: hay movimientos que dependen de él)
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Editar(int id, CategoriaFormViewModel modelo)
        {
            var categoria = await _db.Categorias.FirstOrDefaultAsync(c => c.IdCategoria == id && c.IdUsuario == null);
            if (categoria == null) return NotFound();

            var nombre = modelo.Nombre.Trim();
            if (await _db.Categorias.AnyAsync(c => c.IdUsuario == null && c.Tipo == categoria.Tipo
                                                  && c.Nombre == nombre && c.IdCategoria != categoria.IdCategoria))
                return BadRequest(new ErrorApi { Mensaje = "Ya existe una categoría general con ese nombre.", Campo = "Nombre" });

            categoria.Nombre = nombre;
            categoria.Descripcion = string.IsNullOrWhiteSpace(modelo.Descripcion) ? null : modelo.Descripcion.Trim();
            await _db.SaveChangesAsync();
            return Ok();
        }

        // POST /api/admin/categorias/5/alternar   (activa o desactiva; devuelve el estado resultante)
        [HttpPost("{id:int}/alternar")]
        public async Task<IActionResult> Alternar(int id)
        {
            var categoria = await _db.Categorias.FirstOrDefaultAsync(c => c.IdCategoria == id && c.IdUsuario == null);
            if (categoria == null) return NotFound();

            categoria.Estado = !categoria.Estado;
            await _db.SaveChangesAsync();
            return Ok(categoria.Estado);
        }
    }
}