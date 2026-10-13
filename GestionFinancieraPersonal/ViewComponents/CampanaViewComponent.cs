using Microsoft.AspNetCore.Mvc;
using GestionFinancieraPersonal.Servicios;

namespace GestionFinancieraPersonal.ViewComponents
{
    public class CampanaViewComponent : ViewComponent
    {
        private readonly NotificacionService _servicio;

        public CampanaViewComponent(NotificacionService servicio)
        {
            _servicio = servicio;
        }

        public async Task<IViewComponentResult> InvokeAsync(string claseExtra = "")
        {
            var noLeidas = 0;
            if (UserClaimsPrincipal.Identity?.IsAuthenticated == true)
                noLeidas = await _servicio.ContarNoLeidasAsync(UserClaimsPrincipal.ObtenerIdUsuario());

            return View(new CampanaModelo(noLeidas, claseExtra));
        }
    }

    public record CampanaModelo(int NoLeidas, string ClaseExtra);
}