using GestionFinancieraPersonal.Entidades;
namespace GestionFinancieraPersonal.Servicios
{

    // Decide si un gasto cruzó el umbral configurado por el administrador (ConfiguracionSistema.UmbralPresupuesto)
    // o el 100 % del límite. El umbral nunca se define aquí: siempre llega como parámetro.
    public class AlertasPresupuesto
    {
        // Un presupuesto está "superado" al llegar al 100 % de su límite.
        // No es un ajuste: es la definición de presupuesto agotado (el umbral configurable es otro, siempre menor).
        private const decimal PorcentajeSuperado = 100;

        // % usado del presupuesto (sin redondear)
        public static decimal Uso(Presupuesto p) => p.PorcentajeExacto;

        // Devuelve una notificación solo si el uso CRUZÓ el umbral o el 100 %; si no, null.
        // "Cruzar" = antes estaba por debajo y ahora está en o por encima. Con el uso ya por encima
        // del umbral (72 %, 73 %, ...) usoAntes ya no es menor y no se genera otra notificación.
        public static Notificacion? Evaluar(int idUsuario, string categoria, decimal usoAntes, decimal usoDespues, int umbral)
        {
            if (usoAntes < PorcentajeSuperado && usoDespues >= PorcentajeSuperado)
            {
                return new Notificacion
                {
                    IdUsuario = idUsuario,
                    Tipo = "Presupuesto",
                    Titulo = "Presupuesto superado",
                    Mensaje = $"Superaste el presupuesto de {categoria}: llevas el {Math.Round(usoDespues, 0)}% del límite."
                };
            }

            if (!Presupuesto.Supera(usoAntes, umbral) && Presupuesto.Supera(usoDespues, umbral))
            {
                return new Notificacion
                {
                    IdUsuario = idUsuario,
                    Tipo = "Presupuesto",
                    Titulo = "Alerta de presupuesto",
                    Mensaje = $"Has utilizado más del {umbral}% del presupuesto de {categoria}."
                };
            }

            return null;
        }
    }
}