using GestionFinancieraPersonal.Models;

namespace GestionFinancieraPersonal.Datos
{
    // Único dato que sigue simulado: la conversación del asistente de IA.
    public static class DatosSimulados
    {
        public static List<MensajeChat> Conversacion = new()
        {
            new MensajeChat { Texto = "¿Cuánto he gastado en entretenimiento este mes?", EsDelUsuario = true },
            new MensajeChat { Texto = "Este mes llevas $95.00 en entretenimiento, casi el límite de tu presupuesto ($100.00).", EsDelUsuario = false },
            new MensajeChat { Texto = "¿Qué me recomiendas?", EsDelUsuario = true },
            new MensajeChat { Texto = "Reducir suscripciones no usadas podría liberarte cerca de $15.00 al mes.", EsDelUsuario = false },
        };
    }
}