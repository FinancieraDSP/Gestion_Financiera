using Microsoft.Data.SqlClient;

namespace GestionFinancieraPersonal.Datos
{
    // El script FinanzasPersonales.sql solo se ejecuta cuando la base NO existe.
    // Esta clase pone al día las bases que ya existen (la tuya y las de tus compañeros).
    // Cada paso es idempotente: si ya está aplicado, no hace nada. Se ejecuta en cada arranque.
    // IMPORTANTE: cualquier cambio de esquema nuevo debe ir aquí Y en el script .sql.
    public static class ActualizadorEsquema
    {
        private static readonly string[] Pasos =
        {
            @"IF COL_LENGTH('Usuarios', 'intentos_fallidos') IS NULL
                  ALTER TABLE Usuarios
                  ADD intentos_fallidos INT NOT NULL
                      CONSTRAINT DF_Usuarios_IntentosFallidos DEFAULT 0;",

            @"IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Usuarios_IntentosFallidos')
                  ALTER TABLE Usuarios
                  ADD CONSTRAINT CK_Usuarios_IntentosFallidos CHECK (intentos_fallidos >= 0);",

            @"IF OBJECT_ID('dbo.ConfiguracionSistema', 'U') IS NULL
              BEGIN
                  CREATE TABLE ConfiguracionSistema (
                      id_configuracion INT NOT NULL PRIMARY KEY,
                      umbral_presupuesto INT NOT NULL DEFAULT 85,
                      dias_aviso_deuda INT NOT NULL DEFAULT 3,
                      intentos_permitidos INT NOT NULL DEFAULT 5,
                      mantenimiento_activo BIT NOT NULL DEFAULT 0,
                      fecha_inicio_mantenimiento DATETIME2 NULL,
                      fecha_fin_mantenimiento DATETIME2 NULL,

                      CONSTRAINT CK_Config_UnicaFila CHECK (id_configuracion = 1),
                      CONSTRAINT CK_Config_Umbral CHECK (umbral_presupuesto BETWEEN 50 AND 99),
                      CONSTRAINT CK_Config_DiasAviso CHECK (dias_aviso_deuda BETWEEN 1 AND 30),
                      CONSTRAINT CK_Config_Intentos CHECK (intentos_permitidos BETWEEN 2 AND 10),
                      CONSTRAINT CK_Config_FechasMantenimiento
                          CHECK (fecha_inicio_mantenimiento IS NULL
                                 OR fecha_fin_mantenimiento IS NULL
                                 OR fecha_fin_mantenimiento > fecha_inicio_mantenimiento)
                  );
              END",

            @"IF NOT EXISTS (SELECT 1 FROM ConfiguracionSistema)
                  INSERT INTO ConfiguracionSistema (id_configuracion) VALUES (1);",

            @"IF COL_LENGTH('Deudas', 'aviso_vencimiento_notificado') IS NULL
                ALTER TABLE Deudas
                ADD aviso_vencimiento_notificado DATE NULL;",

            @"IF COL_LENGTH('dbo.Notificaciones', 'eliminada') IS NULL
                ALTER TABLE dbo.Notificaciones
                ADD eliminada BIT NOT NULL
                    CONSTRAINT DF_Notificaciones_eliminada DEFAULT 0;",

            @"IF OBJECT_ID('dbo.PasswordResets', 'U') IS NULL
            BEGIN
                CREATE TABLE PasswordResets (
                    id_reset INT IDENTITY(1,1) PRIMARY KEY,
                    id_usuario INT NOT NULL,
                    token_hash NVARCHAR(255) NOT NULL,
                    fecha_creacion DATETIME2 NOT NULL,
                    fecha_expiracion DATETIME2 NOT NULL,
                    usado BIT NOT NULL DEFAULT 0,
                    CONSTRAINT FK_PasswordResets_Usuarios
                        FOREIGN KEY (id_usuario)
                        REFERENCES Usuarios(id_usuario)
                );
            END"
        };

        public static void Aplicar(string cadenaConexion)
        {
            using var conexion = new SqlConnection(cadenaConexion);
            conexion.Open();

            foreach (var paso in Pasos)
            {
                using var comando = new SqlCommand(paso, conexion);
                comando.ExecuteNonQuery();
            }
        }
    }
}
