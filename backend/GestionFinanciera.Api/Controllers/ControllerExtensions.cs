using GestionFinancieraPersonal.Models;
using GestionFinancieraPersonal.Servicios;
using Microsoft.AspNetCore.Mvc;

namespace GestionFinancieraPersonal.Controllers
{
    public static class ControllerExtensions
    {
        // Exito -> 200. Fallo -> 400 con el mensaje y el campo del formulario.
        public static IActionResult Respuesta(this ControllerBase controlador, ResultadoOperacion resultado) =>
            resultado.Exito
                ? controlador.Ok()
                : controlador.BadRequest(new ErrorApi
                {
                    Mensaje = resultado.Error ?? "No se pudo completar la operación.",
                    Campo = resultado.Campo
                });
    }
}