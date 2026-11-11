namespace Spotta.Application.Comun;

/// <summary>
/// La categoría del rechazo. La API la traduce a un código HTTP; esta capa no
/// sabe qué es HTTP y así no tiene que saberlo.
/// </summary>
public enum TipoDeError
{
    DatosInvalidos,
    NoEncontrado,
    Conflicto,
    Prohibido
}
