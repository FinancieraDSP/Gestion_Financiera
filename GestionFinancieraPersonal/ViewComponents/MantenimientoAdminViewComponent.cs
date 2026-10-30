using Microsoft.AspNetCore.Mvc;
using GestionFinancieraPersonal.Servicios;

namespace GestionFinancieraPersonal.ViewComponents
{
    // Tarjeta de mantenimiento del panel Admin: se inserta con @await Component.InvokeAsync("MantenimientoAdmin")
    public class MantenimientoAdminViewComponent : ViewComponent
    {
        private readonly MantenimientoService _mantenimiento;

        public MantenimientoAdminViewComponent(MantenimientoService mantenimiento)
        {
            _mantenimiento = mantenimiento;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var info = await _mantenimiento.ObtenerAsync();
            return View(new MantenimientoAdminModelo(
                info,
                TempData["MantenimientoExito"] as string,
                TempData["MantenimientoError"] as string));
        }
    }

    public record MantenimientoAdminModelo(InfoMantenimiento Info, string? Exito, string? Error);
}
