using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using GestionFinancieraPersonal.Entidades;

namespace GestionFinancieraPersonal.Models
{
    public class MetaFormViewModel
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100, ErrorMessage = "Máximo 100 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El monto objetivo es obligatorio.")]
        [Range(0.01, 999999999, ErrorMessage = "La meta debe ser mayor a $0.00.")]
        public decimal? MontoObjetivo { get; set; }

        [DataType(DataType.Date)]
        public DateTime? FechaLimite { get; set; }

        public string Icono { get; set; } = "ti-target";
    }

    public class AporteFormViewModel
    {
        [Required(ErrorMessage = "Selecciona una meta.")]
        public int? IdMeta { get; set; }

        [Required(ErrorMessage = "Selecciona la cuenta de origen.")]
        public int? IdCuenta { get; set; }

        [Required(ErrorMessage = "El monto es obligatorio.")]
        [Range(0.01, 999999999, ErrorMessage = "El monto debe ser mayor a $0.00.")]
        public decimal? Monto { get; set; }
    }

    public class AhorroViewModel
    {
        public List<MetaAhorro> Metas { get; set; } = new();
        public decimal AhorroTotal { get; set; }
        public MetaFormViewModel NuevaMeta { get; set; } = new();
        public AporteFormViewModel Aporte { get; set; } = new();
        public List<SelectListItem> Cuentas { get; set; } = new();
        public List<SelectListItem> MetasActivas { get; set; } = new();
    }
}