using Microsoft.EntityFrameworkCore;
using GestionFinancieraPersonal.Models;

namespace GestionFinancieraPersonal.Datos
{
    // NO SE USA TODAVÍA. Queda listo para cuando Jonathan entregue la cadena de
    // conexión a la BDD real. En ese momento:
    //   1) Se registra en Program.cs:
    //        builder.Services.AddDbContext<ApplicationDbContext>(options =>
    //            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
    //   2) Los Controllers dejan de usar DatosSimulados y reciben este contexto
    //      por inyección de dependencias en su constructor.
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Ingreso> Ingresos { get; set; } = null!;
        public DbSet<Gasto> Gastos { get; set; } = null!;
        public DbSet<CategoriaPresupuesto> Presupuestos { get; set; } = null!;
        public DbSet<MetaAhorro> Metas { get; set; } = null!;
        public DbSet<Deuda> Deudas { get; set; } = null!;
        // Agregar/ajustar DbSet conforme al diccionario de datos que entregue Jonathan.
    }
}
