using System.Security.Claims;
namespace GestionFinancieraPersonal.Servicios
{
    public static class ClaimsExtensions
    {
        public static int ObtenerIdUsuario(this ClaimsPrincipal user)
        {
            var valor = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(valor, out var id) ? id : 0;
        }

        public static string ObtenerNombre(this ClaimsPrincipal user)
            => user.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;
    }
}
