using Microsoft.EntityFrameworkCore;
using Spotta.Application.Comun;
using Spotta.Infrastructure;
using Spotta.Infrastructure.Supabase;

namespace Spotta.Application.Usuarios;

/// <summary>La baja es reversible (RN-09): se borra la marca y se desbloquea la cuenta.</summary>
public sealed class ReactivarUsuario(SpottaDb db, ClienteAuthSupabase auth)
{
    public async Task EjecutarAsync(Guid id, Guid autorId, CancellationToken ct)
    {
        var autor = await PerfilActivo.CargarAsync(db, autorId, ct);

        var perfil = await db.Perfiles.FirstOrDefaultAsync(p => p.Id == id && p.GimnasioId == autor.GimnasioId, ct)
                     ?? throw ErroresUsuarios.NoEncontrado();

        if (!perfil.EstaDeBaja) return;

        await auth.DesbloquearAsync(id, ct);

        perfil.DadoDeBajaEn = null;
        perfil.DadoDeBajaPor = null;

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            await Compensacion.DeshacerAsync(
                () => auth.BloquearAsync(id, CancellationToken.None), ex,
                $"la cuenta {id} quedó desbloqueada en Supabase pero el perfil sigue de baja");
            throw;
        }
    }
}
