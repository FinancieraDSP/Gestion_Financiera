using Microsoft.AspNetCore.Mvc.Rendering;
namespace GestionFinancieraPersonal.Models
{
    public class MovimientosViewModel<T>
    {

        public List<T> Items { get; set; } = new();
        public MovimientoFormViewModel Formulario { get; set; } = new();
        public List<SelectListItem> Cuentas { get; set; } = new();
        public List<SelectListItem> Categorias { get; set; } = new();

    }

}
