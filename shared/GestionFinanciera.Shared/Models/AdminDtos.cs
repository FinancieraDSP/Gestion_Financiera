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

    public class ProgramarMantenimientoRequest
    {
        public DateTime Inicio { get; set; }
        public DateTime Fin { get; set; }
    }
}