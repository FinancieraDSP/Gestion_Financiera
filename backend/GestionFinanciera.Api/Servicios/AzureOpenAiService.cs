using System.ClientModel;
using System.Text;
using System.Text.Json;
using Azure.AI.OpenAI;
using GestionFinancieraPersonal.Datos;
using GestionFinancieraPersonal.Entidades;
using Microsoft.EntityFrameworkCore;
using OpenAI.Chat;

namespace GestionFinancieraPersonal.Servicios
{
    public class AzureOpenAiService : IAiService
    {
        private readonly ApplicationDbContext _db;
        private readonly ChatClient _chatClient;
        private readonly ILogger<AzureOpenAiService> _logger;

        private const string SystemPrompt = """
            Eres un asesor financiero personal de la aplicación "Gestión Financiera".
            Tu tono es claro, profesional, motivador y concreto.
            Solo usas los datos del usuario que te proporcionan. Nunca inventes montos, categorías o cuentas que no aparezcan.
            Cuando el usuario gasta mucho en una categoría, sugiere reducirla con un ejemplo concreto.
            Cuando lleva buen progreso en una meta de ahorro, felicítalo y motiva a continuar.
            Responde siempre en español, de forma breve y accionable (máximo 4-5 oraciones).
            Nunca des consejos de inversión de alto riesgo ni menciones bancos reales.
            """;

        public AzureOpenAiService(
            ApplicationDbContext db,
            IConfiguration config,
            ILogger<AzureOpenAiService> logger)
        {
            _db = db;
            _logger = logger;

            var endpoint = config["AzureOpenAI:Endpoint"];
            var apiKey = config["AzureOpenAI:ApiKey"];
            var deployment = config["AzureOpenAI:DeploymentName"];

            if (string.IsNullOrWhiteSpace(endpoint) ||
                string.IsNullOrWhiteSpace(apiKey) ||
                string.IsNullOrWhiteSpace(deployment))
            {
                throw new InvalidOperationException(
                    "Falta configurar AzureOpenAI (Endpoint, ApiKey o DeploymentName).");
            }

            var client = new AzureOpenAIClient(new Uri(endpoint), new ApiKeyCredential(apiKey));
            _chatClient = client.GetChatClient(deployment);
        }

        // ---------- CLASIFICAR GASTO ----------
        public async Task<string> ClasificarGastoAsync(string descripcion, IEnumerable<string> categoriasDisponibles)
        {
            var lista = string.Join(", ", categoriasDisponibles);
            var mensajes = new List<ChatMessage>
            {
                new SystemChatMessage(SystemPrompt),
                new UserChatMessage(
                    $"Clasifica este gasto en UNA sola categoría de la lista.\n" +
                    $"Descripción: \"{descripcion}\"\n" +
                    $"Categorías disponibles: {lista}\n" +
                    $"Responde SOLO con el nombre exacto de la categoría, nada más.")
            };

            try
            {
                var respuesta = await _chatClient.CompleteChatAsync(mensajes);
                return respuesta.Value.Content[0].Text.Trim();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al llamar a Azure OpenAI (Clasificar gasto)");
                return "";
            }
        }

        // ---------- CHAT ----------
        public async Task<string> ChatAsync(int idUsuario, string mensajeUsuario, List<(string Rol, string Texto)>? historial = null)
        {
            var contexto = await ConstruirContextoUsuarioAsync(idUsuario);

            var mensajes = new List<ChatMessage>
            {
                new SystemChatMessage(SystemPrompt + "\n\nDatos actuales del usuario:\n" + contexto)
            };

            if (historial != null)
            {
                foreach (var (rol, texto) in historial.TakeLast(6))
                {
                    if (rol == "user") mensajes.Add(new UserChatMessage(texto));
                    else mensajes.Add(new AssistantChatMessage(texto));
                }
            }

            mensajes.Add(new UserChatMessage(mensajeUsuario));

            try
            {
                var respuesta = await _chatClient.CompleteChatAsync(mensajes);
                return respuesta.Value.Content[0].Text.Trim();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al llamar a Azure OpenAI (Chat)");
                return "Lo siento, el asistente no está disponible en este momento. Intenta más tarde.";
            }
        }

        // ---------- GENERAR RECOMENDACIONES + NOTIFICACIÓN ----------
        public async Task<List<Recomendacion>> GenerarRecomendacionesAsync(int idUsuario)
        {
            var contexto = await ConstruirContextoUsuarioAsync(idUsuario);

            var mensajes = new List<ChatMessage>
            {
                new SystemChatMessage(SystemPrompt),
                new UserChatMessage(
                    $"Con base en estos datos del usuario, genera entre 2 y 4 recomendaciones financieras concretas.\n" +
                    $"Formato de respuesta (JSON estricto, sin texto extra):\n" +
                    $"[ {{\"titulo\": \"...\", \"contenido\": \"...\", \"tipo\": \"Ahorro|Gasto|Presupuesto|General\"}} ]\n\n" +
                    $"Datos:\n{contexto}")
            };

            try
            {
                var respuesta = await _chatClient.CompleteChatAsync(mensajes);
                var texto = respuesta.Value.Content[0].Text.Trim();

                // Limpiar posibles ```json
                if (texto.StartsWith("```"))
                    texto = texto.Replace("```json", "").Replace("```", "").Trim();

                var items = JsonSerializer.Deserialize<List<RecDto>>(texto, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new();

                var recomendaciones = new List<Recomendacion>();

                foreach (var item in items.Take(4))
                {
                    var rec = new Recomendacion
                    {
                        IdUsuario = idUsuario,
                        Titulo = item.Titulo?.Trim() ?? "Recomendación",
                        Contenido = item.Contenido?.Trim() ?? "",
                        Tipo = item.Tipo?.Trim() ?? "General",
                        FechaGeneracion = DateTime.Now,
                        Vista = false
                    };
                    _db.Recomendaciones.Add(rec);
                    recomendaciones.Add(rec);

                    // Notificación para la campanita
                    _db.Notificaciones.Add(new Notificacion
                    {
                        IdUsuario = idUsuario,
                        Titulo = "Nueva recomendación financiera",
                        Mensaje = rec.Titulo,
                        Tipo = "RecomendacionIA",
                        Fecha = DateTime.Now,
                        Leida = false
                    });
                }

                await _db.SaveChangesAsync();
                return recomendaciones;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar recomendaciones");
                return new List<Recomendacion>();
            }
        }

        // ---------- CONTEXTO SOLO DEL USUARIO ACTUAL ----------
        private async Task<string> ConstruirContextoUsuarioAsync(int idUsuario)
        {
            var sb = new StringBuilder();

            var cuentas = await _db.Cuentas
                .Where(c => c.IdUsuario == idUsuario && c.Estado)
                .Select(c => new { c.Nombre, c.SaldoActual })
                .ToListAsync();

            var gastosMes = await _db.Gastos
                .Where(g => g.IdUsuario == idUsuario && g.Fecha >= DateTime.Today.AddDays(-30))
                .GroupBy(g => g.Categoria.Nombre)
                .Select(g => new { Categoria = g.Key, Total = g.Sum(x => x.Monto) })
                .OrderByDescending(x => x.Total)
                .Take(5)
                .ToListAsync();

            var presupuestos = await _db.Presupuestos
                .Where(p => p.IdUsuario == idUsuario && p.Estado)
                .Select(p => new { p.Categoria.Nombre, p.MontoLimite, p.MontoGastado })
                .ToListAsync();

            var metas = await _db.MetasAhorro
                .Where(m => m.IdUsuario == idUsuario && m.Estado == "Activa")
                .Select(m => new { m.Nombre, m.MontoObjetivo, m.MontoActual })
                .ToListAsync();

            sb.AppendLine("Cuentas: " + string.Join(", ", cuentas.Select(c => $"{c.Nombre}: {c.SaldoActual:C}")));
            sb.AppendLine("Gastos últimos 30 días por categoría: " +
                string.Join(", ", gastosMes.Select(g => $"{g.Categoria}: {g.Total:C}")));
            sb.AppendLine("Presupuestos: " +
                string.Join(", ", presupuestos.Select(p => $"{p.Nombre}: {p.MontoGastado:C}/{p.MontoLimite:C}")));
            sb.AppendLine("Metas de ahorro: " +
                string.Join(", ", metas.Select(m => $"{m.Nombre}: {m.MontoActual:C}/{m.MontoObjetivo:C}")));

            return sb.ToString();
        }

        private class RecDto
        {
            public string? Titulo { get; set; }
            public string? Contenido { get; set; }
            public string? Tipo { get; set; }
        }
    }
}
