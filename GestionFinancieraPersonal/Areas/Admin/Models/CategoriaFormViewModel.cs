using System.ComponentModel.DataAnnotations;

namespace GestionFinancieraPersonal.Areas.Admin.Models
{
    public class CategoriaFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100, ErrorMessage = "Máximo 100 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El tipo es obligatorio.")]
        [RegularExpression("^(Gasto|Ingreso)$", ErrorMessage = "Tipo no válido.")]
        public string Tipo { get; set; } = "Gasto";

        [StringLength(200, ErrorMessage = "Máximo 200 caracteres.")]
        public string? Descripcion { get; set; }
    }
}