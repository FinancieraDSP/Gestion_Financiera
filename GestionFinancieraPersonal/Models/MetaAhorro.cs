namespace GestionFinancieraPersonal.Models
{
    public class MetaAhorro
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Icono { get; set; } = "ti-target";
        public DateTime FechaMeta { get; set; }
        public decimal MontoActual { get; set; }
        public decimal MontoObjetivo { get; set; }
        public decimal Porcentaje => MontoObjetivo == 0 ? 0 : Math.Round((MontoActual / MontoObjetivo) * 100, 0);
    }
}
