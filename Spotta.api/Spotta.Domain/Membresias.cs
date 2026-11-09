namespace Spotta.Domain;

/// <summary>
/// El cartel de precios de la pared. Editable; lo ya vendido no cambia (RN-11).
/// No se borra, se archiva (RN-12).
/// </summary>
public sealed class PlanMembresia
{
    public Guid Id { get; set; }
    public Guid GimnasioId { get; set; }

    public required string Nombre { get; set; }
    public int DuracionDias { get; set; }
    public decimal Precio { get; set; }

    public DateTimeOffset? ArchivadoEn { get; set; }
    public DateTimeOffset CreadoEn { get; set; }

    public bool EstaArchivado => ArchivadoEn is not null;
}

/// <summary>
/// El recibo del socio. Copia nombre, duración y precio del plan en el momento
/// de la venta, para que editar el plan no reescriba el pasado (RN-11).
/// El cobro vive aquí: una membresía es un cobro (RN-19).
/// </summary>
public sealed class Membresia
{
    public Guid Id { get; set; }
    public Guid GimnasioId { get; set; }

    public Guid UsuarioId { get; set; }
    public Perfil? Usuario { get; set; }

    public Guid PlanId { get; set; }
    public PlanMembresia? Plan { get; set; }

    // Copia del plan en el momento de la venta
    public required string PlanNombre { get; set; }
    public int DuracionDias { get; set; }

    public DateOnly FechaInicio { get; set; }

    /// <summary>
    /// Almacenada, no calculada al vuelo: los ajustes la mueven dentro de su
    /// misma transacción (RN-15), y así filtrar por vigencia sigue siendo un
    /// predicado sobre fechas que puede usar índices.
    /// </summary>
    public DateOnly FechaFin { get; set; }

    // Cobro
    public decimal PrecioCobrado { get; set; }
    public MetodoPago MetodoPago { get; set; }
    public DateTimeOffset CobradoEn { get; set; }
    public Guid CobradoPor { get; set; }

    public DateTimeOffset CreadoEn { get; set; }

    public ICollection<AjusteMembresia> Ajustes { get; set; } = [];

    /// <summary>El estado no se almacena: se deriva de las fechas (RN-16).</summary>
    public EstadoMembresia EstadoEn(DateOnly hoy) =>
        hoy < FechaInicio ? EstadoMembresia.Programada
        : hoy <= FechaFin ? EstadoMembresia.Vigente
        : EstadoMembresia.Finalizada;

    public int DiasRestantesEn(DateOnly hoy) =>
        Math.Max(0, FechaFin.DayNumber - hoy.DayNumber);
}

public enum EstadoMembresia
{
    Programada,
    Vigente,
    Finalizada
}

/// <summary>
/// Días añadidos (o restados) a mano. No modifican el recibo: se le grapan, con
/// motivo obligatorio y autor, para que conste quién lo autorizó (RN-17).
/// </summary>
public sealed class AjusteMembresia
{
    public Guid Id { get; set; }

    public Guid MembresiaId { get; set; }
    public Membresia? Membresia { get; set; }

    public int Dias { get; set; }
    public required string Motivo { get; set; }

    public DateTimeOffset CreadoEn { get; set; }
    public Guid CreadoPor { get; set; }
}
