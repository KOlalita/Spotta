namespace Spotta.Domain.Usuarios;

/// <summary>
/// Documento de identidad peruano (RN-03): DNI de 8 dígitos o carné de
/// extranjería de 9.
///
/// No se valida dígito verificador: el del DNI peruano no tiene un algoritmo
/// público y estable, y rechazar documentos válidos en el mostrador es peor que
/// aceptar alguno inventado, que se detecta el día que el socio enseña el carné.
/// </summary>
public static class Documento
{
    public const int LargoDni = 8;
    public const int LargoCarneExtranjeria = 9;

    private static readonly char[] Separadores = [' ', '-', '.'];

    /// <summary>
    /// Quita espacios, guiones y puntos. Nada más: si quitara también las
    /// letras, un «12345678A» mal tecleado se guardaría como un DNI perfecto de
    /// 8 dígitos y nadie se enteraría.
    /// </summary>
    public static string Normalizar(string valor) =>
        string.Concat(valor.Where(c => !Separadores.Contains(c)));

    public static bool EsValido(string valor)
    {
        var limpio = Normalizar(valor);
        return limpio.Length is LargoDni or LargoCarneExtranjeria
               && limpio.All(char.IsAsciiDigit);
    }

    public static TipoDocumento TipoDe(string valor) =>
        Normalizar(valor).Length == LargoDni
            ? TipoDocumento.Dni
            : TipoDocumento.CarneExtranjeria;

    /// <summary>Lo que se muestra en los listados del panel (RN-06).</summary>
    public static string Enmascarar(string valor) =>
        valor.Length <= 4 ? valor : string.Concat("····", valor[^4..]);
}
