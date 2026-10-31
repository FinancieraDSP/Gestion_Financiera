using GestionFinancieraPersonal.Models;
using GestionFinancieraPersonal.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GestionFinancieraPersonal.Filtros
{
    // Con mantenimiento EN CURSO, todo usuario autenticado que no sea administrador recibe 423.
    // Los endpoints públicos (login, estado del mantenimiento) no se tocan.
    public class MantenimientoApiFilter : IAsyncActionFilter
    {
        private readonly MantenimientoService _mantenimiento;

        public MantenimientoApiFilter(MantenimientoService mantenimiento)
        {
            _mantenimiento = mantenimiento;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var usuario = context.HttpContext.User;
            var esPublico = context.ActionDescriptor.EndpointMetadata.OfType<IAllowAnonymous>().Any();

            if (esPublico || usuario.Identity?.IsAuthenticated != true || usuario.IsInRole("Administrador"))
            {
                await next();
                return;
            }

            var info = await _mantenimiento.ObtenerAsync();
            if (info.Estado != EstadoMantenimiento.EnCurso)
            {
                await next();
                return;
            }

            context.Result = new ObjectResult(new ErrorApi { Mensaje = "El sistema está en mantenimiento." })
            {
                StatusCode = StatusCodes.Status423Locked
            };
        }
    }
}