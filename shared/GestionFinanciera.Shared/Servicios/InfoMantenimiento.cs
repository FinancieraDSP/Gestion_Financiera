namespace GestionFinancieraPersonal.Servicios
{
    public enum EstadoMantenimiento { SinProgramar, Programado, EnCurso, Finalizado }

    public record InfoMantenimiento(EstadoMantenimiento Estado, DateTime? Inicio, DateTime? Fin);
}