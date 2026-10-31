using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using GestionFinancieraPersonal.Entidades;

namespace GestionFinancieraPersonal.Models
{
    public class DeudaFormViewModel
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(150, ErrorMessage = "Máximo 150 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El acreedor es obligatorio.")]
        [StringLength(150, ErrorMessage = "Máximo 150 caracteres.")]
        public string Acreedor { get; set; } = string.Empty;

        [Required(ErrorMessage = "El monto total es obligatorio.")]
        [Range(0.01, 999999999, ErrorMessage = "El monto debe ser mayor a $0.00.")]
        public decimal? MontoTotal { get; set; }

        [Range(0, 100, ErrorMessage = "El interés debe estar entre 0% y 100%.")]
        public decimal? TasaInteres { get; set; }

        [Required(ErrorMessage = "La fecha de inicio es obligatoria.")]
        [DataType(DataType.Date)]
        public DateTime? FechaInicio { get; set; }

        [DataType(DataType.Date)]
        public DateTime? FechaVencimiento { get; set; }
    }

    public class PagoDeudaFormViewModel
    {
        [Required(ErrorMessage = "Selecciona una deuda.")]
        public int? IdDeuda { get; set; }

        [Required(ErrorMessage = "Selecciona la cuenta de pago.")]
        public int? IdCuenta { get; set; }

        [Required(ErrorMessage = "El monto es obligatorio.")]
        [Range(0.01, 999999999, ErrorMessage = "El monto debe ser mayor a $0.00.")]
        public decimal? Monto { get; set; }

        [DataType(DataType.Date)]
        public DateTime? Fecha { get; set; }

        [StringLength(200, ErrorMessage = "Máximo 200 caracteres.")]
        public string? Descripcion { get; set; }
    }

    public class PagoResumenViewModel
    {
        public string Deuda { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public DateTime Fecha { get; set; }
        public string? Descripcion { get; set; }
    }

    public class DeudasViewModel
    {
        public List<Deuda> Deudas { get; set; } = new();
        public List<PagoResumenViewModel> PagosRecientes { get; set; } = new();
        public decimal DeudaTotal { get; set; }
        public DeudaFormViewModel Nueva { get; set; } = new();
        public PagoDeudaFormViewModel Pago { get; set; } = new();
        public List<SelectListItem> Cuentas { get; set; } = new();
        public List<SelectListItem> DeudasPendientes { get; set; } = new();
    }
}