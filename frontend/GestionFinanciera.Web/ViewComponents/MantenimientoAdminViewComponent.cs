using GestionFinancieraPersonal.Servicios;
using Microsoft.AspNetCore.Mvc;

namespace GestionFinancieraPersonal.ViewComponents
{
    // Tarjeta de mantenimiento del panel Admin: se inserta con @await Component.InvokeAsync("MantenimientoAdmin")
    public class MantenimientoAdminViewComponent : ViewComponent
    {
        private readonly ApiClient _api;

        public MantenimientoAdminViewComponent(ApiClient api)
        {
            _api = api;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var info = (await _api.GetAsync<InfoMantenimiento>("api/mantenimiento/estado")).Requerir();
            return View(new MantenimientoAdminModelo(
                info,
                TempData["MantenimientoExito"] as string,
                TempData["MantenimientoError"] as string));
        }
    }

    public record MantenimientoAdminModelo(InfoMantenimiento Info, string? Exito, string? Error);
}