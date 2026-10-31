using Microsoft.Data.SqlClient;

namespace GestionFinancieraPersonal.Datos
{
    // Azure SQL serverless se pausa por inactividad. La primera conexión después de la pausa
    // la despierta, pero falla con un error transitorio (40613); hay que reintentar.
    // Los errores que NO son transitorios (contraseña mala, firewall) fallan de inmediato.
    public static class ConexionConReintentos
    {
        private static readonly int[] ErroresTransitorios =
        {
            40613, 40197, 40501, 49918, 49919, 49920, 10928, 10929, 10053, 10054, 10060, 64, 233, -2
        };

        public static SqlConnection Abrir(string cadenaConexion, int intentos = 6, int segundosEntreIntentos = 10)
        {
            for (var intento = 1; ; intento++)
            {
                var conexion = new SqlConnection(cadenaConexion);
                try
                {
                    conexion.Open();
                    return conexion;
                }
                catch (SqlException ex) when (intento < intentos && EsTransitorio(ex))
                {
                    conexion.Dispose();
                    Thread.Sleep(TimeSpan.FromSeconds(segundosEntreIntentos));
                }
                catch
                {
                    conexion.Dispose();
                    throw;
                }
            }
        }

        public static async Task<SqlConnection> AbrirAsync(
            string cadenaConexion, CancellationToken cancelacion = default, int intentos = 6, int segundosEntreIntentos = 10)
        {
            for (var intento = 1; ; intento++)
            {
                var conexion = new SqlConnection(cadenaConexion);
                try
                {
                    await conexion.OpenAsync(cancelacion);
                    return conexion;
                }
                catch (SqlException ex) when (intento < intentos && EsTransitorio(ex))
                {
                    await conexion.DisposeAsync();
                    await Task.Delay(TimeSpan.FromSeconds(segundosEntreIntentos), cancelacion);
                }
                catch
                {
                    await conexion.DisposeAsync();
                    throw;
                }
            }
        }

        private static bool EsTransitorio(SqlException ex) =>
            ErroresTransitorios.Contains(ex.Number) ||
            ex.Errors.Cast<SqlError>().Any(e => ErroresTransitorios.Contains(e.Number));
    }
}