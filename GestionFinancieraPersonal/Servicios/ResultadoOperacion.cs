namespace GestionFinancieraPersonal.Servicios
{
    public class ResultadoOperacion
    {
        public bool Exito { get; private init; }
        public string? Error { get; private init; }
        public string Campo { get; private init; } = string.Empty; // campo del formulario al que pertenece el error

        public static ResultadoOperacion Ok() => new() { Exito = true };
        public static ResultadoOperacion Fallo(string error, string campo = "") => new() { Error = error, Campo = campo };
    }
}
