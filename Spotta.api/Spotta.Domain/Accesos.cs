namespace Spotta.Domain;

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

/// <summary>
/// La llave del móvil: el secreto con el que ese teléfono deriva su código TOTP.
/// Se entrega una sola vez y se guarda cifrado (RN-44).
/// </summary>
public sealed class PaseDispositivo
{
    public Guid Id { get; set; }

    public Guid UsuarioId { get; set; }
    public Perfil? Usuario { get; set; }

    public required byte[] SecretoCifrado { get; set; }

    public DateTimeOffset CreadoEn { get; set; }
    public DateTimeOffset? RevocadoEn { get; set; }
    public Guid? RevocadoPor { get; set; }
    public DateTimeOffset? UltimoUsoEn { get; set; }

    public bool EstaRevocado => RevocadoEn is not null;
}

/// <summary>
/// Antirreplay: un código por dispositivo y paso temporal. Sin esta tabla, un QR
/// fotografiado valdría durante su medio minuto de vida. Se limpia cada hora.
/// </summary>
public sealed class PaseConsumido
{
    public Guid DispositivoId { get; set; }

    /// <summary>Número de paso TOTP: segundos Unix divididos entre 30.</summary>
    public long Paso { get; set; }

    public DateTimeOffset ConsumidoEn { get; set; }
}
