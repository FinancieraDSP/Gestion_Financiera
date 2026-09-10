namespace GestionFinancieraPersonal.Models
{
    public class CategoriaPresupuesto
    {
        public string Nombre { get; set; } = string.Empty;
        public decimal Limite { get; set; }
        public decimal Gastado { get; set; }
        public decimal Porcentaje => Limite == 0 ? 0 : Math.Round((Gastado / Limite) * 100, 0);
        public bool EnAlerta => Porcentaje >= 85;
    }
}
