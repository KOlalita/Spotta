using Spotta.Application.Comun;

namespace Spotta.Application.Usuarios;

/// <summary>Todo lo que el módulo de usuarios puede rechazar, con su código estable.</summary>
public static class ErroresUsuarios
{
    public static ErrorDeNegocio DatoObligatorio(string campo) =>
        new(TipoDeError.DatosInvalidos, "dato_obligatorio", $"Falta {campo}.");

    public static ErrorDeNegocio DniInvalido() =>
        new(TipoDeError.DatosInvalidos, "dni_invalido",
            "El documento debe ser un DNI de 8 dígitos o un carné de extranjería de 9.");

    public static ErrorDeNegocio EmailInvalido() =>
        new(TipoDeError.DatosInvalidos, "email_invalido", "El correo no tiene un formato válido.");

    public static ErrorDeNegocio CursorInvalido() =>
        new(TipoDeError.DatosInvalidos, "cursor_invalido", "El cursor de paginación no es válido.");

    public static ErrorDeNegocio EstadoDesconocido(string estado) =>
        new(TipoDeError.DatosInvalidos, "estado_desconocido", $"No existe el estado de membresía «{estado}».");

    public static ErrorDeNegocio NoEncontrado() =>
        new(TipoDeError.NoEncontrado, "usuario_no_encontrado", "No existe ese usuario.");

    public static ErrorDeNegocio DniDuplicado() =>
        new(TipoDeError.Conflicto, "dni_duplicado", "Ya existe un usuario con ese documento (RN-03).");

    public static ErrorDeNegocio EmailDuplicado() =>
        new(TipoDeError.Conflicto, "email_duplicado", "Ya existe una cuenta con ese correo.");

    public static ErrorDeNegocio MembresiaVigente() =>
        new(TipoDeError.Conflicto, "membresia_vigente_existente",
            "No se puede dar de baja a un usuario con una membresía vigente (RN-09).");

    public static ErrorDeNegocio BajaPropia() =>
        new(TipoDeError.Conflicto, "baja_propia",
            "Un administrador no puede darse de baja a sí mismo: dejaría el panel sin nadie que lo deshaga.");

    public static ErrorDeNegocio DeBaja() =>
        new(TipoDeError.Prohibido, "usuario_de_baja", "La cuenta está dada de baja (RN-09).");

    public static ErrorDeNegocio SinPerfil() =>
        new(TipoDeError.Prohibido, "perfil_inexistente",
            "La cuenta existe en Supabase pero no tiene perfil en Spotta.");
}
