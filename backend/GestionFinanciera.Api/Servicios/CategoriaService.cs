using Microsoft.EntityFrameworkCore;
using GestionFinancieraPersonal.Datos;
using GestionFinancieraPersonal.Entidades;
using GestionFinancieraPersonal.Models;

namespace GestionFinancieraPersonal.Servicios
{
    public class CategoriaService
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<CategoriaService> _logger;

        public CategoriaService(ApplicationDbContext db, ILogger<CategoriaService> logger)
        {
            _db = db;
            _logger = logger;
        }

        // Generales activas + todas las propias del usuario (incluso las desactivadas, para poder reactivarlas)
        public Task<List<Categoria>> ObtenerVisiblesAsync(int idUsuario) =>
            _db.Categorias.AsNoTracking()
                .Where(c => c.IdUsuario == idUsuario || (c.IdUsuario == null && c.Estado))
                .OrderBy(c => c.Tipo).ThenBy(c => c.Nombre)
                .ToListAsync();

        public Task<Categoria?> ObtenerPropiaAsync(int idUsuario, int idCategoria) =>
            _db.Categorias.AsNoTracking()
                .FirstOrDefaultAsync(c => c.IdCategoria == idCategoria && c.IdUsuario == idUsuario);

        // ================= CREAR =================

        public async Task<ResultadoOperacion> CrearAsync(int idUsuario, CategoriaUsuarioFormViewModel f)
        {
            try
            {
                var nombre = f.Nombre.Trim();

                if (await ExisteNombreAsync(idUsuario, f.Tipo, nombre, 0))
                    return ResultadoOperacion.Fallo($"Ya existe una categoría de {f.Tipo.ToLower()} llamada \"{nombre}\".", "Nombre");

                _db.Categorias.Add(new Categoria
                {
                    IdUsuario = idUsuario,
                    Nombre = nombre,
                    Tipo = f.Tipo,
                    Descripcion = string.IsNullOrWhiteSpace(f.Descripcion) ? null : f.Descripcion.Trim()
                });
                await _db.SaveChangesAsync();
                return ResultadoOperacion.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear categoría del usuario {IdUsuario}", idUsuario);
                return ResultadoOperacion.Fallo("No se pudo crear la categoría. Intenta de nuevo.");
            }
        }

        // ================= EDITAR (solo nombre y descripción; el tipo no cambia) =================

        public async Task<ResultadoOperacion> EditarAsync(int idUsuario, int idCategoria, string nombre, string? descripcion)
        {
            try
            {
                var categoria = await _db.Categorias
                    .FirstOrDefaultAsync(c => c.IdCategoria == idCategoria && c.IdUsuario == idUsuario);
                if (categoria == null)
                    return ResultadoOperacion.Fallo("La categoría no existe.");

                nombre = nombre.Trim();
                if (await ExisteNombreAsync(idUsuario, categoria.Tipo, nombre, categoria.IdCategoria))
                    return ResultadoOperacion.Fallo($"Ya existe una categoría de {categoria.Tipo.ToLower()} llamada \"{nombre}\".", "Nombre");

                categoria.Nombre = nombre;
                categoria.Descripcion = string.IsNullOrWhiteSpace(descripcion) ? null : descripcion.Trim();
                await _db.SaveChangesAsync();
                return ResultadoOperacion.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al editar categoría {IdCategoria}", idCategoria);
                return ResultadoOperacion.Fallo("No se pudo guardar la categoría.");
            }
        }

        // ================= ELIMINAR (si tiene historial, solo se desactiva) =================

        public async Task<(ResultadoOperacion resultado, bool desactivada)> EliminarAsync(int idUsuario, int idCategoria)
        {
            try
            {
                var categoria = await _db.Categorias
                    .FirstOrDefaultAsync(c => c.IdCategoria == idCategoria && c.IdUsuario == idUsuario);
                if (categoria == null)
                    return (ResultadoOperacion.Fallo("La categoría no existe."), false);

                var enUso =
                    await _db.Gastos.AnyAsync(g => g.IdCategoria == idCategoria) ||
                    await _db.Ingresos.AnyAsync(i => i.IdCategoria == idCategoria) ||
                    await _db.Presupuestos.AnyAsync(p => p.IdCategoria == idCategoria) ||
                    await _db.Recomendaciones.AnyAsync(r => r.IdCategoria == idCategoria);

                if (enUso)
                {
                    categoria.Estado = false; // se conserva el historial
                }
                else
                {
                    _db.Categorias.Remove(categoria);
                }

                await _db.SaveChangesAsync();
                return (ResultadoOperacion.Ok(), enUso);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar categoría {IdCategoria}", idCategoria);
                return (ResultadoOperacion.Fallo("No se pudo eliminar la categoría."), false);
            }
        }

        // ================= REACTIVAR =================

        public async Task<ResultadoOperacion> ReactivarAsync(int idUsuario, int idCategoria)
        {
            try
            {
                var categoria = await _db.Categorias
                    .FirstOrDefaultAsync(c => c.IdCategoria == idCategoria && c.IdUsuario == idUsuario);
                if (categoria == null)
                    return ResultadoOperacion.Fallo("La categoría no existe.");

                categoria.Estado = true;
                await _db.SaveChangesAsync();
                return ResultadoOperacion.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al reactivar categoría {IdCategoria}", idCategoria);
                return ResultadoOperacion.Fallo("No se pudo reactivar la categoría.");
            }
        }

        // Nombre repetido dentro del mismo tipo, contra las generales y las propias
        private Task<bool> ExisteNombreAsync(int idUsuario, string tipo, string nombre, int excluirId) =>
            _db.Categorias.AnyAsync(c =>
                (c.IdUsuario == null || c.IdUsuario == idUsuario) &&
                c.Tipo == tipo && c.Nombre == nombre && c.IdCategoria != excluirId);
    }
}