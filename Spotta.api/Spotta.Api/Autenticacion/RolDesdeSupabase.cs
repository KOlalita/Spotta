using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Spotta.Domain.Usuarios;

namespace Spotta.Api.Autenticacion;

/// <summary>
/// Saca el rol de Spotta de <c>app_metadata.role</c> (RN-07) y lo deja donde lo
/// busca <c>[Authorize(Roles = ...)]</c>.
///
/// Cuidado con el claim <c>role</c> de primer nivel: Supabase lo usa para el rol
/// de Postgres y vale <c>authenticated</c> para cualquiera que haya iniciado
/// sesión. Si la autorización mirase ése, todo usuario pasaría por
/// administrador. Por eso el rol de Spotta va en un claim con nombre propio.
/// <c>app_metadata</c> sólo se escribe con la clave de servicio: el usuario no
/// puede ascenderse a sí mismo.
/// </summary>
public sealed class RolDesdeSupabase : IClaimsTransformation
{
    public const string TipoDeClaim = "spotta_rol";

    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        // Puede llamarse más de una vez por petición: sólo añade el claim la primera
        if (principal.Identity is not ClaimsIdentity { IsAuthenticated: true } identidad
            || identidad.HasClaim(c => c.Type == TipoDeClaim))
            return Task.FromResult(principal);

        var metadatos = identidad.FindFirst("app_metadata")?.Value;
        if (metadatos is null) return Task.FromResult(principal);

        using var json = JsonDocument.Parse(metadatos);
        if (json.RootElement.TryGetProperty("role", out var valor) && valor.ValueKind == JsonValueKind.String)
        {
            // Sólo nombres exactos: Enum.TryParse aceptaría "0" como Administrador
            var rol = Enum.GetNames<Rol>()
                .FirstOrDefault(n => string.Equals(n, valor.GetString(), StringComparison.OrdinalIgnoreCase));

            if (rol is not null) identidad.AddClaim(new Claim(TipoDeClaim, rol));
        }

        return Task.FromResult(principal);
    }
}
