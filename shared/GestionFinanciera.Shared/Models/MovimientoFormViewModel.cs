using System.ComponentModel.DataAnnotations;
namespace GestionFinancieraPersonal.Models
{
    public class MovimientoFormViewModel
    {

        [Required(ErrorMessage = "El monto es obligatorio.")]
        [Range(0.01, 999999999, ErrorMessage = "El monto debe ser mayor a $0.00.")]
        public decimal? Monto { get; set; }

        [Required(ErrorMessage = "La fecha es obligatoria.")]
        [DataType(DataType.Date)]
        public DateTime? Fecha { get; set; }

        [Required(ErrorMessage = "Selecciona una cuenta.")]
        public int? IdCuenta { get; set; }

        [Required(ErrorMessage = "Selecciona una categoría.")]
        public int? IdCategoria { get; set; }

        [Required(ErrorMessage = "La descripción es obligatoria.")]
        [StringLength(200, ErrorMessage = "Máximo 200 caracteres.")]
        public string Descripcion { get; set; } = string.Empty;

        // Origen (ingresos, máx. 100 en la BD) o Comercio (gastos). Opcional.
        [StringLength(100, ErrorMessage = "Máximo 100 caracteres.")]
        public string? Detalle { get; set; }

    }

}
