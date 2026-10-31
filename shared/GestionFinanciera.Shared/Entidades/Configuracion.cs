using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestionFinancieraPersonal.Entidades
{
    // Rangos permitidos para cada ajuste. Se usan en la entidad, en el formulario y en la vista,
    // así el límite está escrito en un solo lugar.
    public static class LimitesConfiguracion
    {
        public const int UmbralMin = 50;
        public const int UmbralMax = 99;
        public const int DiasAvisoMin = 1;
        public const int DiasAvisoMax = 30;
        public const int IntentosMin = 2;
        public const int IntentosMax = 10;
    }

    // Tabla de una sola fila (id_configuracion = 1) con los ajustes del sistema.
    [Table("ConfiguracionSistema")]
    public class Configuracion
    {
        public const int IdUnico = 1;

        [Key, Column("id_configuracion")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)] // la columna no es IDENTITY: siempre vale 1
        public int IdConfiguracion { get; set; } = IdUnico;

        [Column("umbral_presupuesto")]
        [Range(LimitesConfiguracion.UmbralMin, LimitesConfiguracion.UmbralMax,
            ErrorMessage = "El umbral debe estar entre 50 % y 99 %.")]
        public int UmbralPresupuesto { get; set; } = 85;

        [Column("dias_aviso_deuda")]
        [Range(LimitesConfiguracion.DiasAvisoMin, LimitesConfiguracion.DiasAvisoMax,
            ErrorMessage = "Los días de aviso deben estar entre 1 y 30.")]
        public int DiasAvisoDeuda { get; set; } = 3;

        [Column("intentos_permitidos")]
        [Range(LimitesConfiguracion.IntentosMin, LimitesConfiguracion.IntentosMax,
            ErrorMessage = "Los intentos permitidos deben estar entre 2 y 10.")]
        public int IntentosPermitidos { get; set; } = 5;

        // Mantenimiento programado (se usa a partir de la Fase 5)
        [Column("mantenimiento_activo")]
        public bool MantenimientoActivo { get; set; }

        [Column("fecha_inicio_mantenimiento")]
        public DateTime? FechaInicioMantenimiento { get; set; }

        [Column("fecha_fin_mantenimiento")]
        public DateTime? FechaFinMantenimiento { get; set; }
    }
}
