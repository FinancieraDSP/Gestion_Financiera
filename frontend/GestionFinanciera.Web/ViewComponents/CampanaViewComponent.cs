using GestionFinancieraPersonal.Servicios;
using Microsoft.AspNetCore.Mvc;

namespace GestionFinancieraPersonal.ViewComponents
{
    public class CampanaViewComponent : ViewComponent
    {
        private readonly ApiClient _api;

        public CampanaViewComponent(ApiClient api)
        {
            _api = api;
        }

        public async Task<IViewComponentResult> InvokeAsync(string claseExtra = "")
        {
            var noLeidas = 0;

            if (UserClaimsPrincipal.IsInRole("Usuario"))
            {
                try
                {
                    var respuesta = await _api.GetAsync<int>("api/notificaciones/no-leidas");
                    if (respuesta.Exito) noLeidas = respuesta.Datos;
                }
                catch (SesionExpiradaException)
                {
                    // La campana nunca debe tumbar la página; el controlador ya maneja la sesión vencida
                }
            }

            return View(new CampanaModelo(noLeidas, claseExtra));
        }
    }

    public record CampanaModelo(int NoLeidas, string ClaseExtra);
}