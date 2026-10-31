using GestionFinancieraPersonal.Servicios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;

namespace GestionFinancieraPersonal.Filtros
{
    // Con mantenimiento EN CURSO, todo usuario que no sea administrador (anónimo o conectado)
    // es enviado a la pantalla de mantenimiento. La validación fuerte de credenciales la hace la API.
    public class MantenimientoFilter : IAsyncActionFilter
    {
        private readonly ApiClient _api;
        private readonly IMemoryCache _cache;

        public MantenimientoFilter(ApiClient api, IMemoryCache cache)
        {
            _api = api;
            _cache = cache;
        }

        private Task<InfoMantenimiento?> ObtenerInfoAsync() =>
            _cache.GetOrCreateAsync("mantenimiento", async entrada =>
            {
                entrada.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(10);
                var respuesta = await _api.GetAsync<InfoMantenimiento>("api/mantenimiento/estado");
                return respuesta.Exito ? respuesta.Datos : null;
            });

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var http = context.HttpContext;

            // Los administradores nunca se bloquean
            if (http.User.IsInRole("Administrador"))
            {
                await next();
                return;
            }

            var info = await ObtenerInfoAsync();
            if (info == null || info.Estado != EstadoMantenimiento.EnCurso)
            {
                await next();
                return;
            }

            var valores = context.RouteData.Values;
            var area = valores["area"]?.ToString();
            var controlador = valores["controller"]?.ToString();
            var accion = valores["action"]?.ToString();

            // La pantalla de mantenimiento y el cierre de sesión siempre están disponibles
            if (string.IsNullOrEmpty(area) &&
                (controlador == "Mantenimiento" || (controlador == "Account" && accion == "Logout")))
            {
                await next();
                return;
            }

            // Entrada de administradores: /Account/Login?acceso=admin (no hay enlace público a ella).
            // El envío del formulario también pasa: la API solo deja entrar a administradores.
            if (string.IsNullOrEmpty(area) && controlador == "Account" && accion == "Login")
            {
                if ((HttpMethods.IsGet(http.Request.Method) && http.Request.Query["acceso"] == "admin") ||
                    HttpMethods.IsPost(http.Request.Method))
                {
                    await next();
                    return;
                }
            }

            context.Result = new RedirectToActionResult("Index", "Mantenimiento", new { area = "" });
        }
    }
}