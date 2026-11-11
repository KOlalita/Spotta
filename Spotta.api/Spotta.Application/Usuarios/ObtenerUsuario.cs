using Microsoft.EntityFrameworkCore;
using Spotta.Application.Membresias;
using Spotta.Domain.Accesos;
using Spotta.Domain.Membresias;
using Spotta.Domain.Usuarios;
using Spotta.Infrastructure;

namespace Spotta.Application.Usuarios;

public sealed record MembresiaDeLaFicha(
    Guid Id,
    string Plan,
    DateOnly FechaInicio,
    DateOnly FechaFin,
    EstadoMembresia Estado,
    decimal PrecioCobrado,
    MetodoPago MetodoPago);

public sealed record AccesoDeLaFicha(
    DateTimeOffset OcurridoEn,
    ResultadoAcceso Resultado,
    string? Motivo,
    bool Manual);

public sealed record FichaDeUsuario(
    Guid Id,
    string Nombre,
    string Apellidos,
    string Dni,
    string Email,
    Rol Rol,
    bool ContrasenaCambiada,
    DateTimeOffset? DadoDeBajaEn,
    DateTimeOffset CreadoEn,
    SituacionDeMembresia Membresia,
    IReadOnlyList<MembresiaDeLaFicha> Membresias,
    IReadOnlyList<AccesoDeLaFicha> UltimosAccesos);

/// <summary>
/// La ficha que abre recepción. Aquí sí aparece el documento completo (RN-06):
/// el enmascarado es para el listado, que se ve desde el mostrador.
/// </summary>
public sealed class ObtenerUsuario(SpottaDb db, TimeProvider reloj)
{
    private const int AccesosEnLaFicha = 20;

    public async Task<FichaDeUsuario> EjecutarAsync(Guid id, Guid autorId, CancellationToken ct)
    {
        var autor = await PerfilActivo.CargarAsync(db, autorId, ct);

        var perfil = await db.Perfiles.AsNoTracking()
                         .FirstOrDefaultAsync(p => p.Id == id && p.GimnasioId == autor.GimnasioId, ct)
                     ?? throw ErroresUsuarios.NoEncontrado();

        var gimnasio = await db.Gimnasios.AsNoTracking().FirstAsync(g => g.Id == perfil.GimnasioId, ct);
        var hoy = gimnasio.FechaLocal(reloj.GetUtcNow());

        var membresias = await db.Membresias.AsNoTracking()
            .Where(m => m.UsuarioId == id)
            .OrderByDescending(m => m.FechaInicio)
            .ToListAsync(ct);

        var accesos = await db.Accesos.AsNoTracking()
            .Where(a => a.UsuarioId == id)
            .OrderByDescending(a => a.OcurridoEn)
            .Take(AccesosEnLaFicha)
            .Select(a => new AccesoDeLaFicha(a.OcurridoEn, a.Resultado, a.Motivo, a.Manual))
            .ToListAsync(ct);

        return new FichaDeUsuario(
            perfil.Id, perfil.Nombre, perfil.Apellidos, perfil.Dni, perfil.Email, perfil.Rol,
            perfil.ContrasenaCambiada, perfil.DadoDeBajaEn, perfil.CreadoEn,
            SituacionDeMembresia.De(membresias, hoy),
            membresias
                .Select(m => new MembresiaDeLaFicha(
                    m.Id, m.PlanNombre, m.FechaInicio, m.FechaFin, m.EstadoEn(hoy), m.PrecioCobrado, m.MetodoPago))
                .ToList(),
            accesos);
    }
}
