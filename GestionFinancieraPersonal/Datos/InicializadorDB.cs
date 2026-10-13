using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using GestionFinancieraPersonal.Entidades;

namespace GestionFinancieraPersonal.Datos
{
    public static class InicializadorBD
    {
        // Paso 1: crea la BD ejecutando el script SQL si todavía no existe.
        public static void CrearBaseSiNoExiste(string cadenaConexion)
        {
            var builder = new SqlConnectionStringBuilder(cadenaConexion);
            var nombreBD = builder.InitialCatalog;
            builder.InitialCatalog = "master";

            using var conexion = new SqlConnection(builder.ConnectionString);
            conexion.Open();

            using (var consulta = new SqlCommand("SELECT DB_ID(@nombre)", conexion))
            {
                consulta.Parameters.AddWithValue("@nombre", nombreBD);
                var resultado = consulta.ExecuteScalar();
                if (resultado != null && resultado != DBNull.Value) return; // ya existe
            }

            var ruta = Path.Combine(AppContext.BaseDirectory, "Datos", "Script", "FinanzasPersonales.sql");
            if (!File.Exists(ruta))
                throw new FileNotFoundException("No se encontró el script de la base de datos.", ruta);

            var script = File.ReadAllText(ruta);
            var lotes = Regex.Split(script, @"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase);

            foreach (var lote in lotes)
            {
                if (string.IsNullOrWhiteSpace(lote)) continue;
                using var comando = new SqlCommand(lote, conexion);
                comando.ExecuteNonQuery();
            }
        }

        // Paso 2: datos iniciales (solo si la BD está vacía).
        public static void SembrarDatos(ApplicationDbContext db)
        {
            if (db.Roles.Any()) return;

            // Roles
            var rolAdmin = new Rol { Nombre = "Administrador", Descripcion = "Gestiona usuarios y configuración" };
            var rolUsuario = new Rol { Nombre = "Usuario", Descripcion = "Usuario final de la aplicación" };
            db.Roles.AddRange(rolAdmin, rolUsuario);
            db.SaveChanges();

            // Usuarios (contraseñas hasheadas, nunca en texto plano)
            var hasher = new PasswordHasher<Usuario>();
            var admin = new Usuario { IdRol = rolAdmin.IdRol, Nombre = "Administrador", Correo = "admin@finanzapp.com" };
            admin.PasswordHash = hasher.HashPassword(admin, "Admin123*");
            var carlos = new Usuario { IdRol = rolUsuario.IdRol, Nombre = "Carlos Reyes", Correo = "carlos@mail.com" };
            carlos.PasswordHash = hasher.HashPassword(carlos, "Carlos123*");
            db.Usuarios.AddRange(admin, carlos);
            db.SaveChanges();

            // Categorías generales (IdUsuario = null)
            var gastos = new[] { "Alimentación", "Entretenimiento", "Salud", "Educación", "Transporte", "Servicios", "Otros gastos" }
                .Select(n => new Categoria { Nombre = n, Tipo = "Gasto" }).ToList();
            var ingresos = new[] { "Salario", "Freelance", "Otros ingresos" }
                .Select(n => new Categoria { Nombre = n, Tipo = "Ingreso" }).ToList();
            db.Categorias.AddRange(gastos);
            db.Categorias.AddRange(ingresos);
            db.SaveChanges();

            // Cuentas de Carlos
            db.Cuentas.AddRange(
                new Cuenta { IdUsuario = carlos.IdUsuario, Nombre = "Efectivo", Tipo = "Efectivo", SaldoInicial = 150, SaldoActual = 150 },
                new Cuenta { IdUsuario = carlos.IdUsuario, Nombre = "Cuenta principal", Tipo = "Banco", SaldoInicial = 1000, SaldoActual = 1000 },
                new Cuenta { IdUsuario = carlos.IdUsuario, Nombre = "Ahorros", Tipo = "Ahorro", SaldoInicial = 300, SaldoActual = 300 });

            // Presupuestos del mes actual
            var inicioMes = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            var finMes = inicioMes.AddMonths(1).AddDays(-1);
            db.Presupuestos.AddRange(
                new Presupuesto { IdUsuario = carlos.IdUsuario, IdCategoria = gastos.First(c => c.Nombre == "Alimentación").IdCategoria, MontoLimite = 250, PeriodoInicio = inicioMes, PeriodoFin = finMes },
                new Presupuesto { IdUsuario = carlos.IdUsuario, IdCategoria = gastos.First(c => c.Nombre == "Entretenimiento").IdCategoria, MontoLimite = 100, PeriodoInicio = inicioMes, PeriodoFin = finMes },
                new Presupuesto { IdUsuario = carlos.IdUsuario, IdCategoria = gastos.First(c => c.Nombre == "Salud").IdCategoria, MontoLimite = 150, PeriodoInicio = inicioMes, PeriodoFin = finMes });

            // Meta y deuda de ejemplo
            db.MetasAhorro.Add(new MetaAhorro
            {
                IdUsuario = carlos.IdUsuario,
                Nombre = "Teléfono nuevo",
                Icono = "ti-device-mobile",
                MontoObjetivo = 500,
                FechaLimite = new DateTime(2026, 12, 15),
                Estado = "Activa"
            });
            db.Deudas.Add(new Deuda
            {
                IdUsuario = carlos.IdUsuario,
                Nombre = "Tarjeta Visa",
                Acreedor = "Banco",
                MontoTotal = 1450,
                TasaInteres = 2.5m,
                FechaInicio = DateTime.Today.AddMonths(-3),
                FechaVencimiento = DateTime.Today.AddDays(5),
                Estado = "Pendiente"
            });

            db.SaveChanges();
        }
    }
}