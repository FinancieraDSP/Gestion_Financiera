using GestionFinancieraPersonal.Entidades;
using GestionFinancieraPersonal.Models;
using GestionFinancieraPersonal.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace GestionFinancieraPersonal.Controllers
{
    [Authorize(Roles = "Usuario")]
    public class AsistenteIAController : Controller
    {
        private readonly ApiClient _api;

        public AsistenteIAController(ApiClient api)
        {
            _api = api;
        }

        // GET: /AsistenteIA/Recomendaciones
        public async Task<IActionResult> Recomendaciones()
        {
            var resultado = await _api.GetAsync<List<Recomendacion>>("api/asistente/recomendaciones");
            var lista = resultado.Exito ? resultado.Datos ?? new() : new();
            return View(lista);
        }

        // POST: /AsistenteIA/GenerarRecomendaciones
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerarRecomendaciones()
        {
            var resultado = await _api.PostAsync("api/asistente/recomendaciones/generar");
            if (resultado.Exito)
                TempData["Exito"] = "Se generaron nuevas recomendaciones.";
            else
                TempData["Error"] = resultado.Error ?? "No se pudieron generar recomendaciones.";

            return RedirectToAction(nameof(Recomendaciones));
        }

        // GET: /AsistenteIA/Chat
        public IActionResult Chat()
        {
            // Empezamos con un mensaje de bienvenida del bot
            var mensajes = new List<MensajeChat>
            {
                new()
                {
                    Texto = "Hola, soy tu asistente financiero. Puedo ayudarte a entender tus gastos, presupuestos y metas de ahorro. ¿En qué te ayudo?",
                    EsDelUsuario = false
                }
            };
            return View(mensajes);
        }

        // POST: /AsistenteIA/Chat
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Chat(string mensaje)
        {
            var historial = new List<MensajeChat>();

            if (string.IsNullOrWhiteSpace(mensaje))
            {
                historial.Add(new MensajeChat
                {
                    Texto = "Hola, soy tu asistente financiero. ¿En qué te ayudo?",
                    EsDelUsuario = false
                });
                return View(historial);
            }

            // Mensaje del usuario
            historial.Add(new MensajeChat { Texto = mensaje.Trim(), EsDelUsuario = true });

            // Llamada a la API
            var resultado = await _api.PostAsync<ChatRespuesta>("api/asistente/chat", new { Mensaje = mensaje.Trim() });

            if (resultado.Exito && resultado.Datos != null)
            {
                historial.Add(new MensajeChat
                {
                    Texto = resultado.Datos.Respuesta,
                    EsDelUsuario = false
                });
            }
            else
            {
                historial.Add(new MensajeChat
                {
                    Texto = resultado.Error ?? "El asistente no está disponible en este momento. Intenta más tarde.",
                    EsDelUsuario = false
                });
            }
            return View(historial);
        }
    }

    // Clase auxiliar para deserializar la respuesta del chat
    public class ChatRespuesta
    {
        public string Respuesta { get; set; } = string.Empty;
    }
}