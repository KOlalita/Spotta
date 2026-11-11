using Microsoft.EntityFrameworkCore;
using Spotta.Infrastructure;
using Spotta.Infrastructure.Supabase;

namespace Spotta.Application.Usuarios;

/// <summary>
/// La recuperación de contraseña sin correo: el socio que la olvidó va a
/// recepción, y recepción la devuelve al DNI (RN-04). Vuelve el aviso de
/// contraseña por defecto (RN-05), porque vuelve el riesgo que lo motiva.
///
/// Sin compensación: la contraseña anterior no se conoce y no hay a qué volver.
/// Si Postgres falla después de Supabase, la contraseña ya es el DNI pero el
/// aviso no sale; repetir la orden lo arregla, porque es idempotente.
/// </summary>
public sealed class RestablecerContrasena(SpottaDb db, ClienteAuthSupabase auth)
{
    public async Task EjecutarAsync(Guid id, Guid autorId, CancellationToken ct)
    {
        var autor = await PerfilActivo.CargarAsync(db, autorId, ct);

        var perfil = await db.Perfiles.FirstOrDefaultAsync(p => p.Id == id && p.GimnasioId == autor.GimnasioId, ct)
                     ?? throw ErroresUsuarios.NoEncontrado();

        await auth.CambiarContrasenaAsync(id, perfil.Dni, ct);

        perfil.ContrasenaCambiada = false;
        await db.SaveChangesAsync(ct);
    }
}
