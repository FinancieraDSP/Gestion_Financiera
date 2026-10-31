using GestionFinancieraPersonal.Areas.Admin.Models;

namespace GestionFinancieraPersonal.Models
{
    // Datos del dashboard del administrador (antes se pasaban por ViewBag)
    public class AdminDashboardResponse
    {
        public int UsuariosTotales { get; set; }
        public int UsuariosActivos { get; set; }
        public int CategoriasActivas { get; set; }
        public int RegistrosHoy { get; set; }
        public List<int> MovimientosPorMes { get; set; } = new();
        public List<string> MesesEtiquetas { get; set; } = new();
        public List<CategoriaGeneral> Categorias { get; set; } = new();
    }

    // Contraseña temporal generada por el administrador (solo se muestra una vez)
    public class RestablecerContrasenaResponse
    {
        public string Clave { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
    }

    public class ProgramarMantenimientoRequest
    {
        public DateTime Inicio { get; set; }
        public DateTime Fin { get; set; }
    }
}