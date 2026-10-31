namespace GestionFinancieraPersonal.Areas.Admin.Models
{
    public class CategoriaGeneral
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Tipo { get; set; } = "Gasto";   // Gasto | Ingreso
        public bool Activa { get; set; } = true;
        public decimal PorcentajeUso { get; set; }    // % de los gastos de todos los usuarios
    }
}