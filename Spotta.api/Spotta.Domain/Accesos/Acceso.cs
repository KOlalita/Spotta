using Spotta.Domain.Usuarios;

namespace Spotta.Domain.Accesos;

/// <summary>
/// El libro de la puerta. Se guardan también los rechazos, que son justo los que
/// luego hay que explicar (RN-23). De aquí salen el aforo y la racha: ninguno de
/// los dos se almacena.
/// </summary>
public sealed class Acceso
{
    public Guid Id { get; set; }
    public Guid GimnasioId { get; set; }

    public Guid UsuarioId { get; set; }
    public Perfil? Usuario { get; set; }

    /// <summary>Vacío cuando se rechazó por no haber ninguna membresía que apuntar.</summary>
    public Guid? MembresiaId { get; set; }

    public DateTimeOffset OcurridoEn { get; set; }
    public ResultadoAcceso Resultado { get; set; }

    /// <summary>Uno de <see cref="MotivoRechazo"/> cuando el resultado es rechazado.</summary>
    public string? Motivo { get; set; }

    /// <summary>Registrado por DNI desde el panel, sin escanear (RN-26).</summary>
    public bool Manual { get; set; }
    public Guid? RegistradoPor { get; set; }
}
