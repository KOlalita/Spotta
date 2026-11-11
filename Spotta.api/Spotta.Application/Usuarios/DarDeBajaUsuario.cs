using Microsoft.EntityFrameworkCore;
using Spotta.Application.Comun;
using Spotta.Infrastructure;
using Spotta.Infrastructure.Supabase;

namespace Spotta.Application.Usuarios;

/// <summary>
/// Baja lógica (RN-08, RN-09): el usuario no se borra, se marca con fecha y
/// autor y se bloquea su cuenta. Sólo si no tiene una membresía vigente.
/// </summary>
public sealed class DarDeBajaUsuario(SpottaDb db, ClienteAuthSupabase auth, TimeProvider reloj)
{
    public async Task EjecutarAsync(Guid id, Guid autorId, CancellationToken ct)
    {
        var autor = await PerfilActivo.CargarAsync(db, autorId, ct);
        if (id == autor.Id) throw ErroresUsuarios.BajaPropia();

        var perfil = await db.Perfiles.FirstOrDefaultAsync(p => p.Id == id && p.GimnasioId == autor.GimnasioId, ct)
                     ?? throw ErroresUsuarios.NoEncontrado();

        // Ya lo estaba: repetir la orden no cambia nada ni a quién se le atribuye
        if (perfil.EstaDeBaja) return;

        var gimnasio = await db.Gimnasios.AsNoTracking().FirstAsync(g => g.Id == autor.GimnasioId, ct);
        var hoy = gimnasio.FechaLocal(reloj.GetUtcNow());

        if (await db.Membresias.AnyAsync(
                m => m.UsuarioId == id && m.FechaInicio <= hoy && m.FechaFin >= hoy, ct))
            throw ErroresUsuarios.MembresiaVigente();

        await auth.BloquearAsync(id, ct);

        perfil.DadoDeBajaEn = reloj.GetUtcNow();
        perfil.DadoDeBajaPor = autor.Id;

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            await Compensacion.DeshacerAsync(
                () => auth.DesbloquearAsync(id, CancellationToken.None), ex,
                $"la cuenta {id} quedó bloqueada en Supabase pero el perfil sigue activo");
            throw;
        }
    }
}
