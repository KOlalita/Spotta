using Microsoft.EntityFrameworkCore;
using Spotta.Application.Comun;
using Spotta.Infrastructure;
using Spotta.Infrastructure.Supabase;

namespace Spotta.Application.Usuarios;

public sealed record CambiarMiContrasenaSolicitud(string Actual, string Nueva);

/// <summary>
/// El socio cambia su contraseña desde el perfil (RN-04) y desaparece el aviso
/// de contraseña por defecto (RN-05). Se pide la actual: con el móvil
/// desbloqueado en la mano no debería bastar para quedarse con la cuenta.
/// </summary>
public sealed class CambiarMiContrasena(SpottaDb db, ClienteAuthSupabase auth)
{
    public const int LargoMinimo = 8;

    public async Task EjecutarAsync(Guid usuarioId, CambiarMiContrasenaSolicitud solicitud, CancellationToken ct)
    {
        await PerfilActivo.CargarAsync(db, usuarioId, ct);
        var perfil = await db.Perfiles.FirstAsync(p => p.Id == usuarioId, ct);

        var nueva = solicitud.Nueva ?? "";
        if (nueva.Length < LargoMinimo) throw ErroresUsuarios.ContrasenaCorta(LargoMinimo);
        if (nueva == perfil.Dni) throw ErroresUsuarios.ContrasenaEsDni();

        if (!await auth.ComprobarContrasenaAsync(perfil.Email, solicitud.Actual ?? "", ct))
            throw ErroresUsuarios.ContrasenaActualIncorrecta();

        try
        {
            await auth.CambiarContrasenaAsync(usuarioId, nueva, ct);
        }
        catch (ErrorDeSupabase e) when (e.Codigo == "weak_password")
        {
            throw ErroresUsuarios.ContrasenaDebil();
        }

        perfil.ContrasenaCambiada = true;

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            // Aquí sí se puede deshacer del todo: la contraseña anterior se conoce
            await Compensacion.DeshacerAsync(
                () => auth.CambiarContrasenaAsync(usuarioId, solicitud.Actual!, CancellationToken.None), ex,
                $"la cuenta {usuarioId} tiene contraseña nueva pero el perfil no lo sabe");
            throw;
        }
    }
}
