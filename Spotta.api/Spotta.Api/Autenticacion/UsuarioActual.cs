using System.Security.Claims;

namespace Spotta.Api.Autenticacion;

public static class UsuarioActual
{
    /// <summary>
    /// El <c>sub</c> del token: el identificador en <c>auth.users</c>, que es
    /// también el del perfil. Es la única fuente válida para saber quién pide
    /// algo; nunca un parámetro de la URL.
    /// </summary>
    public static Guid Id(this ClaimsPrincipal usuario) =>
        Guid.Parse(usuario.FindFirstValue("sub")
                   ?? throw new InvalidOperationException("El token no trae «sub»."));
}
