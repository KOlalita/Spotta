namespace Spotta.Domain.Notificaciones;

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
