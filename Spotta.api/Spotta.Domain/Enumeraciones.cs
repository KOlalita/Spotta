namespace Spotta.Domain;

public enum Rol
{
    Administrador,
    Cliente
}

public enum MetodoPago
{
    Efectivo,
    Tarjeta,
    Transferencia
}

public enum ResultadoAcceso
{
    Permitido,
    Rechazado
}

/// <summary>
/// Motivos de rechazo de un acceso (RN-23). Son códigos estables: el cliente los
/// traduce a texto, la API nunca manda el mensaje ya redactado.
/// </summary>
public static class MotivoRechazo
{
    public const string PaseInvalido = "pase_invalido";
    public const string CodigoIncorrecto = "codigo_incorrecto";
    public const string PaseYaUsado = "pase_ya_usado";
    public const string DispositivoRevocado = "dispositivo_revocado";
    public const string SinMembresia = "sin_membresia";
    public const string MembresiaVencida = "membresia_vencida";
    public const string UsuarioDeBaja = "usuario_de_baja";
}
