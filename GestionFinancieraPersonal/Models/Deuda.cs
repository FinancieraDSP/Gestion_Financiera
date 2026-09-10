namespace GestionFinancieraPersonal.Models
{
    public class Deuda
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public decimal MontoPendiente { get; set; }
        public DateTime Vencimiento { get; set; }
        public decimal InteresPorcentaje { get; set; }
        public bool PorVencer => (Vencimiento - DateTime.Now).TotalDays <= 7 && (Vencimiento - DateTime.Now).TotalDays >= 0;
    }
}
