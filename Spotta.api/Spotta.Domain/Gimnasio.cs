namespace Spotta.Domain;

/// <summary>
/// El gimnasio y su configuración. Hay uno solo (RN-42), pero todas las tablas
/// de negocio lo referencian desde la primera migración para que pasar a
/// multisede no exija migrar el esquema entero.
/// </summary>
public sealed class Gimnasio
{
    public Guid Id { get; set; }
    public required string Nombre { get; set; }

    /// <summary>Identificador IANA. Manda sobre UTC al calcular días y semanas (RN-31).</summary>
    public string ZonaHoraria { get; set; } = "America/Lima";
    public string Moneda { get; set; } = "PEN";

    /// <summary>Capacidad máxima, sólo informativa: nunca bloquea el acceso (RN-30).</summary>
    public int AforoMaximo { get; set; }

    /// <summary>Minutos que se asume que un socio permanece dentro (RN-28).</summary>
    public int VentanaPresenciaMin { get; set; } = 60;

    /// <summary>Días de asistencia que debe tener una semana para contar (RN-32).</summary>
    public int DiasMinimosRacha { get; set; } = 3;

    public DateTimeOffset CreadoEn { get; set; }
}
