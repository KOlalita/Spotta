using Spotta.Domain.Usuarios;

namespace Spotta.Domain.Notificaciones;

/// <summary>
/// Dispositivo registrado para recibir push. Distinto de
/// <c>PaseDispositivo</c>: aquel genera el código de acceso, este sólo recibe
/// avisos. El interruptor afecta al push, nunca a la bandeja (RN-38).
/// </summary>
public sealed class DispositivoPush
{
    public Guid Id { get; set; }

    public Guid UsuarioId { get; set; }
    public Perfil? Usuario { get; set; }

    public required string ExpoPushToken { get; set; }
    public bool PushActivo { get; set; } = true;
    public DateTimeOffset VistoEn { get; set; }
}
