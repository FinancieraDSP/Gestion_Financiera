using GestionFinancieraPersonal.Models;
using GestionFinancieraPersonal.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestionFinancieraPersonal.Controllers
{
    [Authorize(Roles = "Usuario")]
    [ApiController]
    [Route("api/[controller]")]
    public class AsistenteController : ControllerBase
    {
        private const int MaxCaracteresMensaje = 500;

        private readonly IAiService _ai;

        public AsistenteController(IAiService ai) => _ai = ai;

        // El id sale del token JWT, nunca de lo que envía el navegador
        private int IdUsuario => User.ObtenerIdUsuario();

        [HttpPost("chat")]
        public async Task<IActionResult> Chat([FromBody] ChatRequest? req)
        {
            if (req is null || string.IsNullOrWhiteSpace(req.Mensaje))
                return BadRequest(new ErrorApi { Mensaje = "El mensaje es obligatorio." });

            var mensaje = req.Mensaje.Trim();
            if (mensaje.Length > MaxCaracteresMensaje)
                return BadRequest(new ErrorApi { Mensaje = $"El mensaje no puede superar {MaxCaracteresMensaje} caracteres." });

            var respuesta = await _ai.ChatAsync(IdUsuario, mensaje);
            return Ok(new { respuesta });
        }

        [HttpPost("recomendaciones/generar")]
        public async Task<IActionResult> GenerarRecomendaciones()
        {
            var lista = await _ai.GenerarRecomendacionesAsync(IdUsuario);
            return Ok(lista);
        }

        [HttpGet("recomendaciones")]
        public async Task<IActionResult> ObtenerRecomendaciones([FromServices] Datos.ApplicationDbContext db)
        {
            var lista = await db.Recomendaciones
                .Where(r => r.IdUsuario == IdUsuario)
                .OrderByDescending(r => r.FechaGeneracion)
                .Take(10)
                .ToListAsync();
            return Ok(lista);
        }
    }

    public record ChatRequest(string Mensaje);
}