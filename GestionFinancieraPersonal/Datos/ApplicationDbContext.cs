using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using GestionFinancieraPersonal.Entidades;

namespace GestionFinancieraPersonal.Datos
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Rol> Roles { get; set; } = null!;
        public DbSet<Usuario> Usuarios { get; set; } = null!;
        public DbSet<Cuenta> Cuentas { get; set; } = null!;
        public DbSet<Categoria> Categorias { get; set; } = null!;
        public DbSet<Ingreso> Ingresos { get; set; } = null!;
        public DbSet<Gasto> Gastos { get; set; } = null!;
        public DbSet<Presupuesto> Presupuestos { get; set; } = null!;
        public DbSet<MetaAhorro> MetasAhorro { get; set; } = null!;
        public DbSet<Notificacion> Notificaciones { get; set; } = null!;
        public DbSet<Recomendacion> Recomendaciones { get; set; } = null!;
        public DbSet<Deuda> Deudas { get; set; } = null!;
        public DbSet<PagoDeuda> PagosDeuda { get; set; } = null!;

        // Antes de guardar, se validan las anotaciones de cada entidad nueva o modificada.
        // Si alguna falla se lanza una ValidationException y no se guarda nada.
        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            ValidarEntidades();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            ValidarEntidades();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        private void ValidarEntidades()
        {
            var entidades = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
                .Select(e => e.Entity)
                .ToList();

            foreach (var entidad in entidades)
                Validator.ValidateObject(entidad, new ValidationContext(entidad), validateAllProperties: true);
        }
    }
}