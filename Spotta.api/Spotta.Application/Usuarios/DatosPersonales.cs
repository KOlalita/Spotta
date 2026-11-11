using System.Net.Mail;
using Spotta.Domain.Usuarios;

namespace Spotta.Application.Usuarios;

/// <summary>
/// Nombre, apellidos, documento y correo, ya limpios y validados. Lo comparten
/// el alta y la edición, que exigen exactamente lo mismo (RN-02, RN-03).
/// </summary>
public sealed record DatosPersonales(string Nombre, string Apellidos, string Dni, string Email)
{
    public static DatosPersonales Validar(string? nombre, string? apellidos, string? dni, string? email)
    {
        if (string.IsNullOrWhiteSpace(nombre)) throw ErroresUsuarios.DatoObligatorio("el nombre");
        if (string.IsNullOrWhiteSpace(apellidos)) throw ErroresUsuarios.DatoObligatorio("los apellidos");
        if (string.IsNullOrWhiteSpace(dni)) throw ErroresUsuarios.DatoObligatorio("el documento");
        if (string.IsNullOrWhiteSpace(email)) throw ErroresUsuarios.DatoObligatorio("el correo");

        var documento = Documento.Normalizar(dni);
        if (!Documento.EsValido(documento)) throw ErroresUsuarios.DniInvalido();

        // Supabase compara correos sin distinguir mayúsculas; aquí también
        var correo = email.Trim().ToLowerInvariant();
        if (!MailAddress.TryCreate(correo, out var direccion) || direccion.Address != correo)
            throw ErroresUsuarios.EmailInvalido();

        return new DatosPersonales(nombre.Trim(), apellidos.Trim(), documento, correo);
    }
}
