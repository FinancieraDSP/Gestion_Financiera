using GestionFinancieraPersonal.Entidades;
namespace GestionFinancieraPersonal.Servicios
{

    //Decide si un gasto cruzo el limite
    public class AlertasPresupuesto
    {
        // % usado del presupuesto (sin redondear)
        public static decimal Uso(Presupuesto p) =>
            p.MontoLimite == 0 ? 0 : p.MontoGastado / p.MontoLimite * 100;

        // Devuelve una notificación solo si el uso CRUZÓ el 85% o el 100%; si no, null
        public static Notificacion? Evaluar(int idUsuario, string categoria, decimal usoAntes, decimal usoDespues)
        {
            if (usoAntes < 100 && usoDespues >= 100)
            {
                return new Notificacion
                {
                    IdUsuario = idUsuario,
                    Tipo = "Presupuesto",
                    Titulo = "Presupuesto superado",
                    Mensaje = $"Superaste el presupuesto de {categoria}: llevas el {Math.Round(usoDespues, 0)}% del límite."
                };
            }

            if (usoAntes < Presupuesto.UmbralAlerta && usoDespues >= Presupuesto.UmbralAlerta)
            {
                return new Notificacion
                {
                    IdUsuario = idUsuario,
                    Tipo = "Presupuesto",
                    Titulo = "Alerta de presupuesto",
                    Mensaje = $"Has usado el {Math.Round(usoDespues, 0)}% del presupuesto de {categoria}."
                };
            }

            return null;
        }
    }
}
