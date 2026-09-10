namespace GestionFinancieraPersonal.Areas.Admin.Models
{
    public class CategoriaGeneral
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public decimal PorcentajeUso { get; set; } // uso agregado entre todos los usuarios
    }
}
