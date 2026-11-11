namespace Spotta.Application.Comun;

/// <summary>
/// Una regla de negocio dijo que no. <see cref="Codigo"/> es estable y es lo que
/// los clientes traducen a texto (arquitectura, sección 8): el mensaje sirve
/// para leer los registros, nunca se muestra tal cual en una interfaz.
/// </summary>
public sealed class ErrorDeNegocio(TipoDeError tipo, string codigo, string mensaje)
    : Exception(mensaje)
{
    public TipoDeError Tipo { get; } = tipo;
    public string Codigo { get; } = codigo;
}
