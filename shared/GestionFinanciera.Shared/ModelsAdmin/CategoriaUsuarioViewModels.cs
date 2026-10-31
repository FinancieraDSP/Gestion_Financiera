using System.ComponentModel.DataAnnotations;
using GestionFinancieraPersonal.Entidades;

namespace GestionFinancieraPersonal.Models
{
    public class CategoriaUsuarioFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100, ErrorMessage = "Máximo 100 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "Selecciona el tipo.")]
        [RegularExpression("^(Gasto|Ingreso)$", ErrorMessage = "Tipo no válido.")]
        public string Tipo { get; set; } = "Gasto";

        [StringLength(200, ErrorMessage = "Máximo 200 caracteres.")]
        public string? Descripcion { get; set; }
    }

    public class MisCategoriasViewModel
    {
        public List<Categoria> Categorias { get; set; } = new();
        public CategoriaUsuarioFormViewModel Formulario { get; set; } = new();
    }
}