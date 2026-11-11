namespace Spotta.Domain.Membresias;

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
