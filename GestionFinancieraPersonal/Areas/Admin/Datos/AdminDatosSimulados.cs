using GestionFinancieraPersonal.Areas.Admin.Models;

namespace GestionFinancieraPersonal.Areas.Admin.Datos
{
    // Fuente de datos temporal del panel de Admin, independiente de DatosSimulados
    // del lado usuario (aislamiento propio del Area). Se reemplaza por consultas
    // reales cuando llegue la conexión a BDD.
    public static class AdminDatosSimulados
    {
        public static List<UsuarioAdmin> Usuarios = new()
        {
            new UsuarioAdmin { Id = 1, Nombre = "Carlos Reyes", Correo = "carlos.r@mail.com", FechaRegistro = new DateTime(2026,2,10), UltimoAcceso = new DateTime(2026,7,25), Rol = "Usuario", Activo = true, CategoriasCreadas = 3 },
            new UsuarioAdmin { Id = 2, Nombre = "Ana Morales", Correo = "ana.m@mail.com", FechaRegistro = new DateTime(2026,5,3), UltimoAcceso = new DateTime(2026,7,24), Rol = "Usuario", Activo = true, CategoriasCreadas = 1 },
            new UsuarioAdmin { Id = 3, Nombre = "Jorge Pérez", Correo = "jorge.p@mail.com", FechaRegistro = new DateTime(2025,11,14), UltimoAcceso = new DateTime(2026,7,12), Rol = "Usuario", Activo = false, CategoriasCreadas = 2, MotivoSuspension = "Reportado por actividad inusual" },
        };

        public static List<CategoriaGeneral> Categorias = new()
        {
            new CategoriaGeneral { Id = 1, Nombre = "Alimentación", PorcentajeUso = 38 },
            new CategoriaGeneral { Id = 2, Nombre = "Entretenimiento", PorcentajeUso = 24 },
            new CategoriaGeneral { Id = 3, Nombre = "Salud", PorcentajeUso = 15 },
            new CategoriaGeneral { Id = 4, Nombre = "Educación", PorcentajeUso = 12 },
        };

        public static ConfiguracionSistema Configuracion = new();

        public static int UsuariosTotales => Usuarios.Count;
        public static int UsuariosActivos => Usuarios.Count(u => u.Activo);
        public static int CategoriasActivas => Categorias.Count;

        // Movimientos registrados por todos los usuarios, por mes (para el gráfico del panel general).
        public static List<int> MovimientosPorMes = new() { 180, 210, 195, 240, 260 };
    }
}
