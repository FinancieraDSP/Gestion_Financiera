namespace GestionFinancieraPersonal.Models
{
    // Resultado de eliminar una categoría: si tenía movimientos o presupuestos se desactiva en vez de borrarse
    public class EliminarCategoriaResponse
    {
        public bool Desactivada { get; set; }
    }

    // Resultado de eliminar una meta de ahorro: dinero devuelto y cuenta que lo recibió
    public class EliminarMetaResponse
    {
        public decimal Devuelto { get; set; }
        public string? Cuenta { get; set; }
    }
}