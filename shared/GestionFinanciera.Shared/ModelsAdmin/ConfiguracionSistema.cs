using System.ComponentModel.DataAnnotations;
using GestionFinancieraPersonal.Entidades;

namespace GestionFinancieraPersonal.Areas.Admin.Models
{
    public class ConfiguracionSistema
    {
        [Required(ErrorMessage = "El umbral es obligatorio.")]
        [Range(LimitesConfiguracion.UmbralMin, LimitesConfiguracion.UmbralMax,
            ErrorMessage = "El umbral debe estar entre 50 % y 99 %.")]
        public int? UmbralPresupuesto { get; set; }

        [Required(ErrorMessage = "Los días de aviso son obligatorios.")]
        [Range(LimitesConfiguracion.DiasAvisoMin, LimitesConfiguracion.DiasAvisoMax,
            ErrorMessage = "Los días de aviso deben estar entre 1 y 30.")]
        public int? DiasAvisoDeuda { get; set; }

        [Required(ErrorMessage = "Los intentos permitidos son obligatorios.")]
        [Range(LimitesConfiguracion.IntentosMin, LimitesConfiguracion.IntentosMax,
            ErrorMessage = "Los intentos permitidos deben estar entre 2 y 10.")]
        public int? IntentosPermitidos { get; set; }
    }
}
