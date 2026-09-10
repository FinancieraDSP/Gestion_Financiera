namespace GestionFinancieraPersonal.Models
{
    public class Ingreso
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public string Categoria { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Cuenta { get; set; } = "Cuenta principal";
        public decimal Monto { get; set; }
    }
}
