using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using GestionFinancieraPersonal.Datos;
using GestionFinancieraPersonal.Servicios;

namespace GestionFinancieraPersonal.Filtros
{
    // Filtro global MVC: con mantenimiento EN CURSO, todo usuario que no sea administrador
    // (anónimo o ya conectado) es enviado a la pantalla de mantenimiento en cada petición.
    public class MantenimientoFilter : IAsyncActionFilter
    {
        private readonly MantenimientoService _mantenimiento;
        private readonly ApplicationDbContext _db;

        public MantenimientoFilter(MantenimientoService mantenimiento, ApplicationDbContext db)
        {
            _mantenimiento = mantenimiento;
            _db = db;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var http = context.HttpContext;

            // Los administradores nunca se bloquean
            if (http.User.IsInRole("Administrador"))
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

            var valores = context.RouteData.Values;
            var area = valores["area"]?.ToString();
            var controlador = valores["controller"]?.ToString();
            var accion = valores["action"]?.ToString();

            // La propia pantalla de mantenimiento y el cierre de sesión siempre están disponibles
            if (string.IsNullOrEmpty(area) &&
                (controlador == "Mantenimiento" || (controlador == "Account" && accion == "Logout")))
            {
                await next();
                return;
            }

            if (string.IsNullOrEmpty(area) && controlador == "Account" && accion == "Login")
            {
                // Entrada de administradores: /Account/Login?acceso=admin (no hay enlace público a ella)
                if (HttpMethods.IsGet(http.Request.Method) && http.Request.Query["acceso"] == "admin")
                {
                    await next();
                    return;
                }

                // El envío del formulario solo se procesa si el correo pertenece a un administrador;
                // para cualquier otro no se evalúa ninguna credencial.
                if (HttpMethods.IsPost(http.Request.Method) && http.Request.HasFormContentType)
                {
                    var form = await http.Request.ReadFormAsync();
                    var correo = form["Correo"].ToString().Trim();
                    if (correo.Length > 0 &&
                        await _db.Usuarios.AnyAsync(u => u.Correo == correo && u.Rol.Nombre == "Administrador"))
                    {
                        await next();
                        return;
                    }
                }
            }

            context.Result = new RedirectToActionResult("Index", "Mantenimiento", new { area = "" });
        }
    }
}
