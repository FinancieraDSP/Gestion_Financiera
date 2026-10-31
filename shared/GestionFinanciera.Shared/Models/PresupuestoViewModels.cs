using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using GestionFinancieraPersonal.Entidades;

namespace GestionFinancieraPersonal.Models
{
    public class PresupuestoFormViewModel
    {
        [Required(ErrorMessage = "Selecciona una categoría.")]
        public int? IdCategoria { get; set; }

        [Required(ErrorMessage = "El límite es obligatorio.")]
        [Range(0.01, 999999999, ErrorMessage = "El límite debe ser mayor a $0.00.")]
        public decimal? Limite { get; set; }
    }

    public class PresupuestoViewModel
    {
        public string Periodo { get; set; } = string.Empty;
        public List<Presupuesto> Presupuestos { get; set; } = new();
        public PresupuestoFormViewModel Formulario { get; set; } = new();
        public List<SelectListItem> Categorias { get; set; } = new();
    }
}
