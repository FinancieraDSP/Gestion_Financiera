using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using GestionFinancieraPersonal.Entidades;

namespace GestionFinancieraPersonal.Datos
{
    public static class InicializadorBD
    {
        // Paso 1: deja la BD lista, con sus tablas.
        public static void CrearBaseSiNoExiste(string cadenaConexion)
        {
            var builder = new SqlConnectionStringBuilder(cadenaConexion);

            // AZURE SQL: la base ya la creó el portal (CREATE DATABASE y USE no están permitidos).
            // Solo se crean las tablas si la base todavía está vacía.
            if (EsAzureSql(builder))
            {
                using var azure = ConexionConReintentos.Abrir(cadenaConexion);
                if (ExisteTabla(azure, "PagosDeuda")) return; // el esquema ya está creado
                EjecutarScript(azure, omitirCreacionDeBD: true);
                return;
            }

            // LOCAL (LocalDB / SQL Server): igual que antes
            var nombreBD = builder.InitialCatalog;
            builder.InitialCatalog = "master";

            using var conexion = ConexionConReintentos.Abrir(builder.ConnectionString);

            using (var consulta = new SqlCommand("SELECT DB_ID(@nombre)", conexion))
            {
                consulta.Parameters.AddWithValue("@nombre", nombreBD);
                var resultado = consulta.ExecuteScalar();
                if (resultado != null && resultado != DBNull.Value) return; // ya existe
            }

            EjecutarScript(conexion, omitirCreacionDeBD: false);
        }

        private static bool EsAzureSql(SqlConnectionStringBuilder builder) =>
            builder.DataSource.Contains(".database.windows.net", StringComparison.OrdinalIgnoreCase);

        private static bool ExisteTabla(SqlConnection conexion, string tabla)
        {
            using var comando = new SqlCommand("SELECT OBJECT_ID(@tabla, 'U')", conexion);
            comando.Parameters.AddWithValue("@tabla", "dbo." + tabla);
            var resultado = comando.ExecuteScalar();
            return resultado != null && resultado != DBNull.Value;
        }

        private static void EjecutarScript(SqlConnection conexion, bool omitirCreacionDeBD)
        {
            var ruta = Path.Combine(AppContext.BaseDirectory, "Datos", "Script", "FinanzasPersonales.sql");
            if (!File.Exists(ruta))
                throw new FileNotFoundException("No se encontró el script de la base de datos.", ruta);

            var script = File.ReadAllText(ruta);
            var lotes = Regex.Split(script, @"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase);

            foreach (var lote in lotes)
            {
                if (string.IsNullOrWhiteSpace(lote)) continue;

                // En Azure no existen CREATE DATABASE ni USE: la base ya viene creada
                if (omitirCreacionDeBD &&
                    Regex.IsMatch(lote, @"^\s*(CREATE\s+DATABASE|USE)\b", RegexOptions.IgnoreCase))
                    continue;

                using var comando = new SqlCommand(lote, conexion);
                comando.ExecuteNonQuery();
            }
        }

        // Paso 2: datos mínimos para que la aplicación pueda usarse. NO hay datos de ejemplo:
        // todo lo demás (movimientos, presupuestos, metas, deudas...) lo agregan los usuarios.
        public static void SembrarDatos(ApplicationDbContext db, IConfiguration configuracion, ILogger logger)
        {
            // 1) Roles: imprescindibles
            if (!db.Roles.Any())
            {
                db.Roles.AddRange(
                    new Rol { Nombre = "Administrador", Descripcion = "Gestiona usuarios y configuración" },
                    new Rol { Nombre = "Usuario", Descripcion = "Usuario final de la aplicación" });
                db.SaveChanges();
            }

            /* 2) Categorías generales (IdUsuario = null): catálogo base que ve todo el mundo.
            //    El administrador puede crear más desde su panel.
            //    Si NO las quieres, borra este bloque completo.
            if (!db.Categorias.Any(c => c.IdUsuario == null))
            {
                var gastos = new[] { "Alimentación", "Entretenimiento", "Salud", "Educación", "Transporte", "Servicios", "Otros gastos" }
                    .Select(n => new Categoria { Nombre = n, Tipo = "Gasto" });
                var ingresos = new[] { "Salario", "Freelance", "Otros ingresos" }
                    .Select(n => new Categoria { Nombre = n, Tipo = "Ingreso" });
                db.Categorias.AddRange(gastos);
                db.Categorias.AddRange(ingresos);
                db.SaveChanges();
            }*/

            // 3) Usuarios iniciales: SOLO si la base no tiene ningún usuario.
            //    (Así, si el admin elimina al cliente inicial, no reaparece al reiniciar.)
            if (db.Usuarios.Any()) return;

            var hasher = new PasswordHasher<Usuario>();

            Usuario? CrearUsuarioInicial(string seccion, string nombreRol)
            {
                var s = configuracion.GetSection(seccion);
                var nombre = s["Nombre"];
                var correo = s["Correo"];
                var contrasena = s["Contrasena"];

                if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(correo) || string.IsNullOrWhiteSpace(contrasena))
                {
                    logger.LogWarning(
                        "No se creó el usuario inicial '{Seccion}': falta configurar Nombre, Correo y Contrasena.", seccion);
                    return null;
                }

                var rol = db.Roles.First(r => r.Nombre == nombreRol);
                var usuario = new Usuario { IdRol = rol.IdRol, Nombre = nombre.Trim(), Correo = correo.Trim() };
                usuario.PasswordHash = hasher.HashPassword(usuario, contrasena); // nunca se guarda en texto plano
                db.Usuarios.Add(usuario);
                return usuario;
            }

            CrearUsuarioInicial("SeedUsuarios:Admin", "Administrador");
            var cliente = CrearUsuarioInicial("SeedUsuarios:Cliente", "Usuario");
            db.SaveChanges();

            // El cliente inicial arranca como cualquier usuario recién registrado: dos cuentas en $0
            if (cliente != null)
            {
                db.Cuentas.AddRange(
                    new Cuenta { IdUsuario = cliente.IdUsuario, Nombre = "Efectivo", Tipo = "Efectivo", SaldoInicial = 0, SaldoActual = 0 },
                    new Cuenta { IdUsuario = cliente.IdUsuario, Nombre = "Cuenta principal", Tipo = "Banco", SaldoInicial = 0, SaldoActual = 0 });
                db.SaveChanges();
            }
        }
    }
}