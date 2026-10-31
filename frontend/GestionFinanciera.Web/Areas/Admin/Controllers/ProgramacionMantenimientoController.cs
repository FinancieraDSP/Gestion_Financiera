using System.ComponentModel.DataAnnotations;
using GestionFinancieraPersonal.Models;
using GestionFinancieraPersonal.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionFinancieraPersonal.Areas.Admin.Controllers
{
    public class MantenimientoForm
    {
        [Required] public DateTime? FechaInicio { get; set; }
        [Required] public TimeSpan? HoraInicio { get; set; }
        [Required] public DateTime? FechaFin { get; set; }
        [Required] public TimeSpan? HoraFin { get; set; }
    }

    [Area("Admin")]
    [Authorize(Roles = "Administrador")]
    public class ProgramacionMantenimientoController : Controller
    {
        private readonly ApiClient _api;

        public ProgramacionMantenimientoController(ApiClient api)
        {
            _api = api;
        }

        // POST: /Admin/ProgramacionMantenimiento/Guardar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Guardar(MantenimientoForm f)
        {
            if (!ModelState.IsValid)
            {
                TempData["MantenimientoError"] = "Completa fecha y hora de inicio y de finalización.";
                return Volver();
            }

            var inicio = f.FechaInicio!.Value.Date + f.HoraInicio!.Value;
            var fin = f.FechaFin!.Value.Date + f.HoraFin!.Value;

            var r = await _api.PostAsync("api/admin/mantenimiento",
                new ProgramarMantenimientoRequest { Inicio = inicio, Fin = fin });
            Notificar(r, "Programación de mantenimiento guardada.");
            return Volver();
        }

        // POST: /Admin/ProgramacionMantenimiento/Cancelar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancelar()
        {
            Notificar(await _api.PostAsync("api/admin/mantenimiento/cancelar"), "Programación cancelada.");
            return Volver();
        }

        // POST: /Admin/ProgramacionMantenimiento/Finalizar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Finalizar()
        {
            Notificar(await _api.PostAsync("api/admin/mantenimiento/finalizar"),
                "Mantenimiento finalizado. Los usuarios ya pueden ingresar.");
            return Volver();
        }

        private void Notificar(ApiResultado r, string exito)
        {
            if (r.Exito) TempData["MantenimientoExito"] = exito;
            else TempData["MantenimientoError"] = r.Error;
        }

        private IActionResult Volver() => RedirectToAction("Index", "Configuracion", new { area = "Admin" });
    }
}