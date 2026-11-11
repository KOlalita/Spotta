namespace Spotta.Domain.Notificaciones;

/// <summary>Quién leyó qué y cuándo, para el contador de no leídas (RN-39).</summary>
public sealed class NotificacionLectura
{
    public Guid NotificacionId { get; set; }
    public Guid UsuarioId { get; set; }
    public DateTimeOffset LeidaEn { get; set; }
}
