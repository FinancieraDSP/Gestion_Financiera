namespace GestionFinancieraPersonal.Models
{
    public class DashboardViewModel
    {
        public string NombreUsuario { get; set; } = string.Empty;
        public string PeriodoActual { get; set; } = string.Empty;
        public decimal SaldoDisponible { get; set; }
        public decimal TotalIngresos { get; set; }
        public decimal TotalGastos { get; set; }
        public decimal AhorroMensual { get; set; }
        public Dictionary<string, decimal> GastosPorCategoria { get; set; } = new();
        public List<Gasto> UltimosGastos { get; set; } = new();
    }
}
