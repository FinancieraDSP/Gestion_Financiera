using GestionFinancieraPersonal.Models;

namespace GestionFinancieraPersonal.Datos
{
    // Fuente de datos temporal mientras llega la conexión real a la BDD de Jonathan.
    // Cuando esté lista, se reemplaza por ApplicationDbContext (EF Core) sin que
    // los Controllers ni las Views tengan que cambiar de forma (mismos tipos de Model).
    public static class DatosSimulados
    {
        public static List<Ingreso> Ingresos = new()
        {
            new Ingreso { Id = 1, Fecha = new DateTime(2026,7,22), Categoria = "Salario", Descripcion = "Pago mensual", Cuenta = "Cuenta principal", Monto = 1200.00m },
            new Ingreso { Id = 2, Fecha = new DateTime(2026,7,15), Categoria = "Freelance", Descripcion = "Proyecto diseño web", Cuenta = "Cuenta principal", Monto = 300.00m },
        };

        public static List<Gasto> Gastos = new()
        {
            new Gasto { Id = 1, Fecha = new DateTime(2026,7,22), Categoria = "Alimentación", Descripcion = "Supermercado La Colonia", MetodoPago = "Tarjeta", Monto = 85.40m },
            new Gasto { Id = 2, Fecha = new DateTime(2026,7,20), Categoria = "Entretenimiento", Descripcion = "Suscripción Netflix", MetodoPago = "Transferencia", Monto = 13.99m },
            new Gasto { Id = 3, Fecha = new DateTime(2026,7,18), Categoria = "Salud", Descripcion = "Farmacia San Nicolás", MetodoPago = "Efectivo", Monto = 22.00m },
        };

        public static List<CategoriaPresupuesto> Presupuesto = new()
        {
            new CategoriaPresupuesto { Nombre = "Alimentación", Limite = 250, Gastado = 210 },
            new CategoriaPresupuesto { Nombre = "Entretenimiento", Limite = 100, Gastado = 95 },
            new CategoriaPresupuesto { Nombre = "Salud", Limite = 150, Gastado = 60 },
            new CategoriaPresupuesto { Nombre = "Educación", Limite = 200, Gastado = 95 },
        };

        public static List<MetaAhorro> Metas = new()
        {
            new MetaAhorro { Id = 1, Nombre = "Teléfono nuevo", Icono = "ti-device-mobile", FechaMeta = new DateTime(2026,12,15), MontoActual = 310, MontoObjetivo = 500 },
            new MetaAhorro { Id = 2, Nombre = "Viaje de fin de año", Icono = "ti-plane", FechaMeta = new DateTime(2026,12,20), MontoActual = 280, MontoObjetivo = 1000 },
        };

        public static List<Deuda> Deudas = new()
        {
            new Deuda { Id = 1, Nombre = "Tarjeta Visa", MontoPendiente = 1450, Vencimiento = new DateTime(2026,7,27), InteresPorcentaje = 2.5m },
            new Deuda { Id = 2, Nombre = "Préstamo personal", MontoPendiente = 800, Vencimiento = new DateTime(2026,8,5), InteresPorcentaje = 1.8m },
            new Deuda { Id = 3, Nombre = "Cuota laptop", MontoPendiente = 90, Vencimiento = new DateTime(2026,8,15), InteresPorcentaje = 0m },
        };

        public static List<MensajeChat> Conversacion = new()
        {
            new MensajeChat { Texto = "¿Cuánto he gastado en entretenimiento este mes?", EsDelUsuario = true },
            new MensajeChat { Texto = "Este mes llevas $95.00 en entretenimiento, casi el límite de tu presupuesto ($100.00).", EsDelUsuario = false },
            new MensajeChat { Texto = "¿Qué me recomiendas?", EsDelUsuario = true },
            new MensajeChat { Texto = "Reducir suscripciones no usadas podría liberarte cerca de $15.00 al mes.", EsDelUsuario = false },
        };

        public static decimal TotalIngresos => Ingresos.Sum(i => i.Monto);
        public static decimal TotalGastos => Gastos.Sum(g => g.Monto);
        public static decimal SaldoDisponible => TotalIngresos - TotalGastos;
        public static decimal AhorroTotal => Metas.Sum(m => m.MontoActual);

        public static Dictionary<string, decimal> GastosPorCategoria()
            => Gastos.GroupBy(g => g.Categoria).ToDictionary(g => g.Key, g => g.Sum(x => x.Monto));
    }
}
