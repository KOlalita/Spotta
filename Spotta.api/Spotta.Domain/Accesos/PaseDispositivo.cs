using Spotta.Domain.Usuarios;

namespace Spotta.Domain.Accesos;

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
