namespace Spotta.Domain.Membresias;

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
