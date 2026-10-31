using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using GestionFinancieraPersonal.Models;
using Microsoft.AspNetCore.Authentication;

namespace GestionFinancieraPersonal.Servicios
{
    public class SesionExpiradaException : Exception
    {
        public SesionExpiradaException() : base("La sesión expiró o ya no es válida.") { }
    }

    // Error de la API en una operación que no tiene alternativa (ej. cargar una pantalla).
    // Termina en la página amigable de error; el detalle queda en el log.
    public class ApiException : Exception
    {
        public int Codigo { get; }
        public ApiException(int codigo, string mensaje) : base(mensaje) { Codigo = codigo; }
    }

    public class ApiResultado
    {
        public bool Exito { get; init; }
        public int Codigo { get; init; }
        public string? Error { get; init; }
        public string Campo { get; init; } = string.Empty;
    }

    public class ApiResultado<T> : ApiResultado
    {
        public T? Datos { get; init; }

        // Devuelve los datos o lanza ApiException si la llamada falló
        public T Requerir() =>
            Exito && Datos is not null
                ? Datos
                : throw new ApiException(Codigo, Error ?? "Respuesta inesperada de la API.");
    }

    // Único punto de contacto del frontend con el backend:
    // agrega el token JWT, serializa JSON y traduce los errores a mensajes.
    public class ApiClient
    {
        private static readonly JsonSerializerOptions OpcionesJson = new(JsonSerializerDefaults.Web);

        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _contexto;
        private readonly ILogger<ApiClient> _logger;

        public ApiClient(HttpClient http, IHttpContextAccessor contexto, ILogger<ApiClient> logger)
        {
            _http = http;
            _contexto = contexto;
            _logger = logger;
        }

        public Task<ApiResultado<T>> GetAsync<T>(string url) => EnviarAsync<T>(HttpMethod.Get, url, null);
        public Task<ApiResultado<T>> PostAsync<T>(string url, object? cuerpo = null) => EnviarAsync<T>(HttpMethod.Post, url, cuerpo);
        public Task<ApiResultado<T>> PutAsync<T>(string url, object? cuerpo = null) => EnviarAsync<T>(HttpMethod.Put, url, cuerpo);
        public Task<ApiResultado<T>> DeleteAsync<T>(string url) => EnviarAsync<T>(HttpMethod.Delete, url, null);

        public async Task<ApiResultado> PostAsync(string url, object? cuerpo = null) => await EnviarAsync<object>(HttpMethod.Post, url, cuerpo);
        public async Task<ApiResultado> PutAsync(string url, object? cuerpo = null) => await EnviarAsync<object>(HttpMethod.Put, url, cuerpo);
        public async Task<ApiResultado> DeleteAsync(string url) => await EnviarAsync<object>(HttpMethod.Delete, url, null);

        private async Task<ApiResultado<T>> EnviarAsync<T>(HttpMethod metodo, string url, object? cuerpo)
        {
            var http = _contexto.HttpContext;
            var token = http == null ? null : await http.GetTokenAsync("access_token");

            using var peticion = new HttpRequestMessage(metodo, url);
            if (!string.IsNullOrEmpty(token))
                peticion.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            if (cuerpo != null)
                peticion.Content = JsonContent.Create(cuerpo, cuerpo.GetType(), options: OpcionesJson);

            try
            {
                using var respuesta = await _http.SendAsync(peticion);
                var codigo = (int)respuesta.StatusCode;
                var texto = await respuesta.Content.ReadAsStringAsync();

                if (respuesta.IsSuccessStatusCode)
                {
                    T? datos = default;
                    if (!string.IsNullOrWhiteSpace(texto))
                        datos = JsonSerializer.Deserialize<T>(texto, OpcionesJson);
                    return new ApiResultado<T> { Exito = true, Codigo = codigo, Datos = datos };
                }

                // 401 con token enviado = sesión vencida o usuario bloqueado (el login anónimo no entra aquí)
                if (codigo == 401 && !string.IsNullOrEmpty(token))
                    throw new SesionExpiradaException();

                var (mensaje, campo) = ExtraerError(texto, codigo);
                return new ApiResultado<T> { Codigo = codigo, Error = mensaje, Campo = campo };
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                _logger.LogError(ex, "No se pudo comunicar con la API ({Metodo} {Url})", metodo, url);
                return new ApiResultado<T>
                {
                    Codigo = 503,
                    Error = "No se pudo comunicar con el servidor. Intenta de nuevo en unos momentos."
                };
            }
        }

        private static (string mensaje, string campo) ExtraerError(string texto, int codigo)
        {
            try
            {
                var error = JsonSerializer.Deserialize<ErrorApi>(texto, OpcionesJson);
                if (error != null && !string.IsNullOrWhiteSpace(error.Mensaje))
                    return (error.Mensaje, error.Campo ?? string.Empty);
            }
            catch (JsonException) { }

            var generico = codigo switch
            {
                400 => "Los datos enviados no son válidos.",
                403 => "No tienes permiso para realizar esta acción.",
                404 => "No se encontró lo solicitado.",
                _ => "Ocurrió un error al procesar la solicitud."
            };
            return (generico, string.Empty);
        }
    }
}