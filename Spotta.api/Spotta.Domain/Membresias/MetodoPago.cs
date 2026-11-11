namespace Spotta.Domain.Membresias;

/// <summary>Cómo se cobró la membresía (RN-19). Spotta no procesa pagos: los anota.</summary>
public enum MetodoPago
{
    Efectivo,
    Tarjeta,
    Transferencia
}
