namespace Spotta.Domain.Accesos;

/// <summary>
/// Antirreplay: un código por dispositivo y paso temporal. Sin esta tabla, un QR
/// fotografiado valdría durante su medio minuto de vida.
/// </summary>
public sealed class PaseConsumido
{
    public Guid DispositivoId { get; set; }

    /// <summary>Número de paso TOTP: segundos Unix divididos entre 30.</summary>
    public long Paso { get; set; }

    public DateTimeOffset ConsumidoEn { get; set; }
}
