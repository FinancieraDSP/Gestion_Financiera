using GestionFinancieraPersonal.Entidades;
using GestionFinancieraPersonal.Models;

namespace GestionFinancieraPersonal.Datos
{
    // Datos simulados del módulo Asistente IA (chat y recomendaciones).
    public static class DatosSimulados
    {
        public static List<MensajeChat> Conversacion = new()
        {
            new MensajeChat { Texto = "¿Cuánto he gastado en entretenimiento este mes?", EsDelUsuario = true },
            new MensajeChat { Texto = "Este mes llevas $95.00 en entretenimiento, casi el límite de tu presupuesto ($100.00).", EsDelUsuario = false },
            new MensajeChat { Texto = "¿Qué me recomiendas?", EsDelUsuario = true },
            new MensajeChat { Texto = "Reducir suscripciones no usadas podría liberarte cerca de $15.00 al mes.", EsDelUsuario = false },
        };

        public static List<Recomendacion> Recomendaciones = new()
        {
            new Recomendacion { Tipo = "Gasto", Titulo = "Reduce el gasto en entretenimiento",
                Contenido = "Vas en $95 de $100 asignados. Bajar $20 este mes te acerca a tu meta de ahorro.",
                FechaGeneracion = DateTime.Today },
            new Recomendacion { Tipo = "Ahorro", Titulo = "Aumenta tu ahorro mensual",
                Contenido = "Con tu ingreso actual podrías subir de $430 a $500 sin afectar tus gastos fijos.",
                FechaGeneracion = DateTime.Today.AddDays(-1) },
            new Recomendacion { Tipo = "Recurrente", Titulo = "Gasto recurrente detectado",
                Contenido = "Netflix se repite hace 5 meses ($13.99). Revisa si aún la usas.",
                FechaGeneracion = DateTime.Today.AddDays(-2) },
            new Recomendacion { Tipo = "Deuda", Titulo = "Pago próximo: Tarjeta Visa",
                Contenido = "Tu Tarjeta Visa vence en 2 días. Pagar a tiempo evita intereses del 2.5%.",
                FechaGeneracion = DateTime.Today.AddDays(-3) },
            new Recomendacion { Tipo = "Presupuesto", Titulo = "Alimentación al 84% del límite",
                Contenido = "Llevas $210 de $250 en alimentación. Planifica tus compras del resto del mes.",
                FechaGeneracion = DateTime.Today.AddDays(-4) },
        };
    }
}