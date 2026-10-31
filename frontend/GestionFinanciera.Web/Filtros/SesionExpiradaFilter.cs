using GestionFinancieraPersonal.Servicios;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GestionFinancieraPersonal.Filtros
{
    // - API responde 401 (token vencido o usuario bloqueado): cierra sesión y manda al login.
    // - API responde 423 (mantenimiento en curso): manda a la pantalla de mantenimiento.
    public class SesionExpiradaFilter : IAsyncExceptionFilter
    {
        public async Task OnExceptionAsync(ExceptionContext context)
        {
            switch (context.Exception)
            {
                case SesionExpiradaException:
                    await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                    context.Result = new RedirectToActionResult("Login", "Account", new { area = "" });
                    context.ExceptionHandled = true;
                    break;

                case ApiException { Codigo: 423 }:
                    context.Result = new RedirectToActionResult("Index", "Mantenimiento", new { area = "" });
                    context.ExceptionHandled = true;
                    break;
            }
        }
    }
}