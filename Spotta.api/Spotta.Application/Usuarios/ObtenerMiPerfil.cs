using Microsoft.EntityFrameworkCore;
using Spotta.Application.Membresias;
using Spotta.Infrastructure;

namespace Spotta.Application.Usuarios;

public sealed record MiPerfil(
    string Nombre,
    string Apellidos,
    string Dni,
    string Email,
    bool ContrasenaCambiada,
    string Gimnasio,
    SituacionDeMembresia Membresia);

/// <summary>
/// Lo que el socio ve de sí mismo (RN-40). El identificador sale del token,
/// nunca de la URL: así nadie puede pedir el perfil de otro cambiando un número.
/// </summary>
public sealed class ObtenerMiPerfil(SpottaDb db, TimeProvider reloj)
{
    public async Task<MiPerfil> EjecutarAsync(Guid usuarioId, CancellationToken ct)
    {
        var perfil = await PerfilActivo.CargarAsync(db, usuarioId, ct);

        var gimnasio = await db.Gimnasios.AsNoTracking().FirstAsync(g => g.Id == perfil.GimnasioId, ct);
        var hoy = gimnasio.FechaLocal(reloj.GetUtcNow());

        var membresias = await db.Membresias.AsNoTracking()
            .Where(m => m.UsuarioId == usuarioId)
            .ToListAsync(ct);

        return new MiPerfil(
            perfil.Nombre, perfil.Apellidos, perfil.Dni, perfil.Email, perfil.ContrasenaCambiada,
            gimnasio.Nombre, SituacionDeMembresia.De(membresias, hoy));
    }
}
