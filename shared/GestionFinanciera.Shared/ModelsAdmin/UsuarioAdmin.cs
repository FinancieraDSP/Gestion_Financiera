namespace GestionFinancieraPersonal.Areas.Admin.Models
{
    public class UsuarioAdmin
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public DateTime FechaRegistro { get; set; }
        public DateTime? UltimoAcceso { get; set; }
        public string Rol { get; set; } = "Usuario";
        public bool Activo { get; set; } = true;
        public int CategoriasCreadas { get; set; }
        public string? MotivoSuspension { get; set; }

        public string Iniciales =>
            string.Join("", Nombre.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                                  .Take(2)
                                  .Select(p => p[0]))
            .ToUpper();
    }
}
