using Microsoft.EntityFrameworkCore;
using Spotta.Domain.Usuarios;
using Spotta.Infrastructure;

namespace Spotta.Application.Usuarios;

/// <summary>
/// Quien hace la petición, garantizado activo.
///
/// Dar de baja bloquea la cuenta en Supabase, pero eso sólo impide iniciar
/// sesión y renovar el token: el que ya tiene en la mano vale hasta una hora más.
/// Por eso la baja se comprueba aquí, en cada petición, y no sólo en el login.
/// Cuesta una lectura por clave primaria, que de todas formas hacía falta para
/// saber a qué gimnasio pertenece.
/// </summary>
public static class PerfilActivo
{
    public static async Task<Perfil> CargarAsync(SpottaDb db, Guid id, CancellationToken ct)
    {
        var perfil = await db.Perfiles.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, ct)
                     ?? throw ErroresUsuarios.SinPerfil();

        if (perfil.EstaDeBaja) throw ErroresUsuarios.DeBaja();

        return perfil;
    }
}
