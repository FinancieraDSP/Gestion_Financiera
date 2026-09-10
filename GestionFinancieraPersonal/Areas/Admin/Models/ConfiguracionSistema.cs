namespace GestionFinancieraPersonal.Areas.Admin.Models
{
    public class ConfiguracionSistema
    {
        public int UmbralAlertaPresupuesto { get; set; } = 85; // %
        public int DiasAvisoVencimientoDeudas { get; set; } = 3;
        public int MinutosSesionInactiva { get; set; } = 15;
        public int IntentosAccesoPermitidos { get; set; } = 5;
        public bool ModoMantenimiento { get; set; } = false;
        public bool RespaldoAutomatico { get; set; } = true;
    }
}
