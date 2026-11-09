namespace Spotta.Domain;

/// <summary>
/// Aviso que recepción manda a todos los socios. Se persiste siempre, aunque el
/// envío push falle: la bandeja de la aplicación no depende de la entrega (RN-37).
/// </summary>
public sealed class Notificacion
{
    public Guid Id { get; set; }
    public Guid GimnasioId { get; set; }

    public required string Titulo { get; set; }
    public required string Cuerpo { get; set; }

    public DateTimeOffset EnviadaEn { get; set; }
    public Guid EnviadaPor { get; set; }
}

/// <summary>Quién leyó qué y cuándo, para el contador de no leídas (RN-39).</summary>
public sealed class NotificacionLectura
{
    public Guid NotificacionId { get; set; }
    public Guid UsuarioId { get; set; }
    public DateTimeOffset LeidaEn { get; set; }
}

/// <summary>
/// Dispositivo registrado para recibir push. Distinto de
/// <see cref="PaseDispositivo"/>: aquel genera el código de acceso, este sólo
/// recibe avisos. El interruptor afecta al push, nunca a la bandeja (RN-38).
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
