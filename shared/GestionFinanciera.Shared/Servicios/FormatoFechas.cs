namespace GestionFinancieraPersonal.Servicios
{
    public static class FormatoFechas
    {
        private static readonly string[] Meses =
        {
            "enero", "febrero", "marzo", "abril", "mayo", "junio",
            "julio", "agosto", "septiembre", "octubre", "noviembre", "diciembre"
        };

        public static string MesAnio(DateTime fecha) => $"{Meses[fecha.Month - 1]} {fecha.Year}";

        public static string MesCorto(DateTime fecha)
        {
            var mes = Meses[fecha.Month - 1];
            return char.ToUpper(mes[0]) + mes.Substring(1, 2); // "Jul", "Ago"...
        }

    }
}
