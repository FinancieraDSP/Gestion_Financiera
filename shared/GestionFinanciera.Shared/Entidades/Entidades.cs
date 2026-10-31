using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestionFinancieraPersonal.Entidades
{
    // Tope razonable para montos DECIMAL(18,2)
    public static class LimitesModelo
    {
        public const double Monto = 999999999999999;
    }

    [Table("Roles")]
    public class Rol
    {
        [Key, Column("id_rol")]
        public int IdRol { get; set; }

        [Column("nombre")]
        [Required(ErrorMessage = "El nombre del rol es obligatorio.")]
        [StringLength(50, ErrorMessage = "El nombre del rol admite máximo 50 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        [Column("descripcion")]
        [StringLength(200, ErrorMessage = "La descripción admite máximo 200 caracteres.")]
        public string? Descripcion { get; set; }
    }

    [Table("Usuarios")]
    public class Usuario
    {
        [Key, Column("id_usuario")]
        public int IdUsuario { get; set; }

        [Column("id_rol")]
        public int IdRol { get; set; }

        [Column("nombre")]
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre admite máximo 100 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        [Column("correo")]
        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "El correo no es válido.")]
        [StringLength(150, ErrorMessage = "El correo admite máximo 150 caracteres.")]
        public string Correo { get; set; } = string.Empty;

        [Column("password_hash")]
        [Required(ErrorMessage = "La contraseña (hash) es obligatoria.")]
        [StringLength(255)]
        public string PasswordHash { get; set; } = string.Empty;

        [Column("fecha_registro")]
        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        [Column("estado")]
        public bool Estado { get; set; } = true;

        [ForeignKey(nameof(IdRol))]
        public Rol Rol { get; set; } = null!;

        [Column("intentos_fallidos")]
        public int IntentosFallidos { get; set; }
    }

    [Table("Cuentas")]
    public class Cuenta
    {
        [Key, Column("id_cuenta")]
        public int IdCuenta { get; set; }

        [Column("id_usuario")]
        public int IdUsuario { get; set; }

        [Column("nombre")]
        [Required(ErrorMessage = "El nombre de la cuenta es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre admite máximo 100 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        [Column("tipo")]
        [Required]
        [RegularExpression("^(Efectivo|Banco|Ahorro|Tarjeta)$", ErrorMessage = "Tipo de cuenta no válido.")]
        public string Tipo { get; set; } = "Banco";

        [Column("saldo_inicial", TypeName = "decimal(18,2)")]
        [Range(0, LimitesModelo.Monto, ErrorMessage = "El saldo inicial no puede ser negativo.")]
        public decimal SaldoInicial { get; set; }

        [Column("saldo_actual", TypeName = "decimal(18,2)")]
        [Range(0, LimitesModelo.Monto, ErrorMessage = "El saldo no puede quedar negativo.")]
        public decimal SaldoActual { get; set; }

        [Column("fecha_creacion")]
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        [Column("estado")]
        public bool Estado { get; set; } = true;
    }

    [Table("Categorias")]
    public class Categoria
    {
        [Key, Column("id_categoria")]
        public int IdCategoria { get; set; }

        [Column("id_usuario")]
        public int? IdUsuario { get; set; }   // null = categoría general

        [Column("nombre")]
        [Required(ErrorMessage = "El nombre de la categoría es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre admite máximo 100 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        [Column("tipo")]
        [Required]
        [RegularExpression("^(Ingreso|Gasto)$", ErrorMessage = "El tipo debe ser Ingreso o Gasto.")]
        public string Tipo { get; set; } = "Gasto";

        [Column("descripcion")]
        [StringLength(200, ErrorMessage = "La descripción admite máximo 200 caracteres.")]
        public string? Descripcion { get; set; }

        [Column("estado")]
        public bool Estado { get; set; } = true;
    }

    [Table("Ingresos")]
    public class Ingreso
    {
        [Key, Column("id_ingreso")]
        public int IdIngreso { get; set; }

        [Column("id_usuario")]
        public int IdUsuario { get; set; }

        [Column("id_cuenta")]
        public int IdCuenta { get; set; }

        [Column("id_categoria")]
        public int IdCategoria { get; set; }

        [Column("descripcion")]
        [Required(ErrorMessage = "La descripción es obligatoria.")]
        [StringLength(200, ErrorMessage = "La descripción admite máximo 200 caracteres.")]
        public string Descripcion { get; set; } = string.Empty;

        [Column("monto", TypeName = "decimal(18,2)")]
        [Range(0.01, LimitesModelo.Monto, ErrorMessage = "El monto debe ser mayor a $0.00.")]
        public decimal Monto { get; set; }

        [Column("fecha", TypeName = "date")]
        public DateTime Fecha { get; set; }

        [Column("origen")]
        [StringLength(100, ErrorMessage = "El origen admite máximo 100 caracteres.")]
        public string? Origen { get; set; }

        [ForeignKey(nameof(IdCuenta))]
        public Cuenta Cuenta { get; set; } = null!;

        [ForeignKey(nameof(IdCategoria))]
        public Categoria Categoria { get; set; } = null!;
    }

    [Table("Gastos")]
    public class Gasto
    {
        [Key, Column("id_gasto")]
        public int IdGasto { get; set; }

        [Column("id_usuario")]
        public int IdUsuario { get; set; }

        [Column("id_cuenta")]
        public int IdCuenta { get; set; }

        [Column("id_categoria")]
        public int IdCategoria { get; set; }

        [Column("descripcion")]
        [Required(ErrorMessage = "La descripción es obligatoria.")]
        [StringLength(200, ErrorMessage = "La descripción admite máximo 200 caracteres.")]
        public string Descripcion { get; set; } = string.Empty;

        [Column("monto", TypeName = "decimal(18,2)")]
        [Range(0.01, LimitesModelo.Monto, ErrorMessage = "El monto debe ser mayor a $0.00.")]
        public decimal Monto { get; set; }

        [Column("fecha", TypeName = "date")]
        public DateTime Fecha { get; set; }

        [Column("comercio")]
        [StringLength(150, ErrorMessage = "El comercio admite máximo 150 caracteres.")]
        public string? Comercio { get; set; }

        [ForeignKey(nameof(IdCuenta))]
        public Cuenta Cuenta { get; set; } = null!;

        [ForeignKey(nameof(IdCategoria))]
        public Categoria Categoria { get; set; } = null!;
    }

    [Table("Presupuestos")]
    public class Presupuesto : IValidatableObject
    {
        [Key, Column("id_presupuesto")]
        public int IdPresupuesto { get; set; }

        [Column("id_usuario")]
        public int IdUsuario { get; set; }

        [Column("id_categoria")]
        public int IdCategoria { get; set; }

        [Column("monto_limite", TypeName = "decimal(18,2)")]
        [Range(0.01, LimitesModelo.Monto, ErrorMessage = "El límite debe ser mayor a $0.00.")]
        public decimal MontoLimite { get; set; }

        [Column("monto_gastado", TypeName = "decimal(18,2)")]
        [Range(0, LimitesModelo.Monto, ErrorMessage = "Lo gastado no puede ser negativo.")]
        public decimal MontoGastado { get; set; }

        [Column("periodo_inicio", TypeName = "date")]
        public DateTime PeriodoInicio { get; set; }

        [Column("periodo_fin", TypeName = "date")]
        public DateTime PeriodoFin { get; set; }

        [Column("estado")]
        public bool Estado { get; set; } = true;

        [ForeignKey(nameof(IdCategoria))]
        public Categoria Categoria { get; set; } = null!;

        // % redondeado: solo para mostrar en pantalla (barras, etiquetas)
        [NotMapped] public decimal Porcentaje => MontoLimite == 0 ? 0 : Math.Round(MontoGastado / MontoLimite * 100, 0);

        // % sin redondear: es el que se compara contra el umbral (el mismo valor que usan las notificaciones)
        [NotMapped] public decimal PorcentajeExacto => MontoLimite == 0 ? 0 : MontoGastado / MontoLimite * 100;

        // Umbral vigente (ConfiguracionSistema.UmbralPresupuesto). Lo asigna PresupuestoService.ObtenerDelMesAsync;
        // no se guarda en la tabla y no tiene valor por defecto: si nadie lo asigna, EnAlerta es false.
        [NotMapped] public int? UmbralAplicado { get; set; }

        [NotMapped] public bool EnAlerta => UmbralAplicado.HasValue && Supera(PorcentajeExacto, UmbralAplicado.Value);

        // Única regla de comparación con el umbral (la usan EnAlerta y las notificaciones).
        // ">=": entra en alerta al llegar al umbral. Para exigir "estrictamente más", cambiar solo este operador.
        public static bool Supera(decimal uso, int umbral) => uso >= umbral;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (PeriodoFin < PeriodoInicio)
                yield return new ValidationResult("El fin del periodo no puede ser anterior al inicio.", new[] { nameof(PeriodoFin) });
        }
    }

    [Table("MetasAhorro")]
    public class MetaAhorro : IValidatableObject
    {
        [Key, Column("id_meta")]
        public int IdMeta { get; set; }

        [Column("id_usuario")]
        public int IdUsuario { get; set; }

        [Column("id_cuenta")]
        public int? IdCuenta { get; set; }

        [Column("nombre")]
        [Required(ErrorMessage = "El nombre de la meta es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre admite máximo 100 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        [Column("descripcion")]
        [StringLength(250, ErrorMessage = "La descripción admite máximo 250 caracteres.")]
        public string? Descripcion { get; set; }

        [Column("icono")]
        [StringLength(50)]
        public string Icono { get; set; } = "ti-target";

        [Column("monto_objetivo", TypeName = "decimal(18,2)")]
        [Range(0.01, LimitesModelo.Monto, ErrorMessage = "La meta debe ser mayor a $0.00.")]
        public decimal MontoObjetivo { get; set; }

        [Column("monto_actual", TypeName = "decimal(18,2)")]
        [Range(0, LimitesModelo.Monto, ErrorMessage = "Lo ahorrado no puede ser negativo.")]
        public decimal MontoActual { get; set; }

        [Column("fecha_limite", TypeName = "date")]
        public DateTime? FechaLimite { get; set; }

        [Column("estado")]
        [Required]
        [RegularExpression("^(Activa|Completada|Cancelada)$", ErrorMessage = "Estado de meta no válido.")]
        public string Estado { get; set; } = "Activa";

        [NotMapped] public decimal Porcentaje => MontoObjetivo == 0 ? 0 : Math.Round(MontoActual / MontoObjetivo * 100, 0);

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (MontoActual > MontoObjetivo)
                yield return new ValidationResult("Lo ahorrado no puede superar el objetivo.", new[] { nameof(MontoActual) });
        }
    }

    [Table("Notificaciones")]
    public class Notificacion
    {
        [Key, Column("id_notificacion")]
        public int IdNotificacion { get; set; }

        [Column("id_usuario")]
        public int IdUsuario { get; set; }

        [Column("titulo")]
        [Required]
        [StringLength(150, ErrorMessage = "El título admite máximo 150 caracteres.")]
        public string Titulo { get; set; } = string.Empty;

        [Column("mensaje")]
        [Required]
        [StringLength(500, ErrorMessage = "El mensaje admite máximo 500 caracteres.")]
        public string Mensaje { get; set; } = string.Empty;

        [Column("tipo")]
        [Required]
        [StringLength(50)]
        public string Tipo { get; set; } = string.Empty;

        [Column("fecha")]
        public DateTime Fecha { get; set; } = DateTime.Now;

        [Column("leida")]
        public bool Leida { get; set; }

        [Column("eliminada")]
        public bool Eliminada { get; set; }
    }

    [Table("Recomendaciones")]
    public class Recomendacion
    {
        [Key, Column("id_recomendacion")]
        public int IdRecomendacion { get; set; }

        [Column("id_usuario")]
        public int IdUsuario { get; set; }

        [Column("id_categoria")]
        public int? IdCategoria { get; set; }

        [Column("titulo")]
        [Required]
        [StringLength(150, ErrorMessage = "El título admite máximo 150 caracteres.")]
        public string Titulo { get; set; } = string.Empty;

        [Column("contenido")]
        [Required]
        [StringLength(1000, ErrorMessage = "El contenido admite máximo 1000 caracteres.")]
        public string Contenido { get; set; } = string.Empty;

        [Column("tipo")]
        [Required]
        [StringLength(50)]
        public string Tipo { get; set; } = string.Empty;

        [Column("fecha_generacion")]
        public DateTime FechaGeneracion { get; set; } = DateTime.Now;

        [Column("vista")]
        public bool Vista { get; set; }
    }

    [Table("Deudas")]
    public class Deuda : IValidatableObject
    {
        [Key, Column("id_deuda")]
        public int IdDeuda { get; set; }

        [Column("id_usuario")]
        public int IdUsuario { get; set; }

        [Column("id_cuenta")]
        public int? IdCuenta { get; set; }

        [Column("nombre")]
        [Required(ErrorMessage = "El nombre de la deuda es obligatorio.")]
        [StringLength(150, ErrorMessage = "El nombre admite máximo 150 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        [Column("acreedor")]
        [Required(ErrorMessage = "El acreedor es obligatorio.")]
        [StringLength(150, ErrorMessage = "El acreedor admite máximo 150 caracteres.")]
        public string Acreedor { get; set; } = string.Empty;

        [Column("monto_total", TypeName = "decimal(18,2)")]
        [Range(0.01, LimitesModelo.Monto, ErrorMessage = "El monto total debe ser mayor a $0.00.")]
        public decimal MontoTotal { get; set; }

        [Column("monto_pagado", TypeName = "decimal(18,2)")]
        [Range(0, LimitesModelo.Monto, ErrorMessage = "Lo pagado no puede ser negativo.")]
        public decimal MontoPagado { get; set; }

        [Column("tasa_interes", TypeName = "decimal(5,2)")]
        [Range(0, 999.99, ErrorMessage = "La tasa de interés no es válida.")]
        public decimal TasaInteres { get; set; }

        [Column("fecha_inicio", TypeName = "date")]
        public DateTime FechaInicio { get; set; }

        [Column("fecha_vencimiento", TypeName = "date")]
        public DateTime? FechaVencimiento { get; set; }

        // Fecha de vencimiento para la que ya se envió el aviso "Deuda por vencer".
        // Evita duplicados: se avisa solo si es distinta de FechaVencimiento.
        [Column("aviso_vencimiento_notificado", TypeName = "date")]
        public DateTime? AvisoVencimientoNotificado { get; set; }

        [Column("estado")]
        [Required]
        [RegularExpression("^(Pendiente|En proceso|Pagada|Vencida)$", ErrorMessage = "Estado de deuda no válido.")]
        public string Estado { get; set; } = "Pendiente";

        [NotMapped] public decimal MontoPendiente => MontoTotal - MontoPagado;

        // Ventana vigente (ConfiguracionSistema.DiasAvisoDeuda). Lo asigna DeudaService.ObtenerAsync;
        // no se guarda en la tabla y no tiene valor por defecto: si nadie lo asigna, PorVencer es false.
        [NotMapped] public int? DiasAvisoAplicado { get; set; }

        [NotMapped]
        public bool PorVencer =>
            FechaVencimiento.HasValue && DiasAvisoAplicado.HasValue &&
            (FechaVencimiento.Value.Date - DateTime.Today).TotalDays >= 0 &&
            (FechaVencimiento.Value.Date - DateTime.Today).TotalDays <= DiasAvisoAplicado.Value;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (MontoPagado > MontoTotal)
                yield return new ValidationResult("Lo pagado no puede superar el total de la deuda.", new[] { nameof(MontoPagado) });
        }
    }

    [Table("PagosDeuda")]
    public class PagoDeuda
    {
        [Key, Column("id_pago")]
        public int IdPago { get; set; }

        [Column("id_deuda")]
        public int IdDeuda { get; set; }

        [Column("id_cuenta")]
        public int IdCuenta { get; set; }

        [Column("monto", TypeName = "decimal(18,2)")]
        [Range(0.01, LimitesModelo.Monto, ErrorMessage = "El monto debe ser mayor a $0.00.")]
        public decimal Monto { get; set; }

        [Column("fecha_pago", TypeName = "date")]
        public DateTime FechaPago { get; set; }

        [Column("descripcion")]
        [StringLength(200, ErrorMessage = "La descripción admite máximo 200 caracteres.")]
        public string? Descripcion { get; set; }
    }
[Table("PasswordResets")]
public class PasswordReset
{
    [Key]
    [Column("id_reset")]
    public int IdReset { get; set; }

    [Column("id_usuario")]
    public int IdUsuario { get; set; }

    [Column("token_hash")]
    [Required]
    [StringLength(255)]
    public string TokenHash { get; set; } = string.Empty;

    [Column("fecha_creacion")]
    public DateTime FechaCreacion { get; set; }

    [Column("fecha_expiracion")]
    public DateTime FechaExpiracion { get; set; }

    [Column("usado")]
    public bool Usado { get; set; }

    [ForeignKey(nameof(IdUsuario))]
    public Usuario Usuario { get; set; } = null!;
    }
}