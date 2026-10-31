using GestionFinancieraPersonal.Entidades;
using GestionFinancieraPersonal.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionFinancieraPersonal.Controllers
{
    // PLACEHOLDER: el equipo de IA reemplaza esto por llamadas a la API (ver README, sección "Asistente IA").
    [Authorize(Roles = "Usuario")]
    public class AsistenteIAController : Controller
    {
        public IActionResult Recomendaciones()
        {
            return View(new List<Recomendacion>());   // la vista ya tiene su mensaje de "aún no hay recomendaciones"
        }

        public IActionResult Chat()
        {
            return View(new List<MensajeChat>
            {
                new() { Texto = "El asistente financiero estará disponible próximamente.", EsDelUsuario = false }
            });
        }
    }
}