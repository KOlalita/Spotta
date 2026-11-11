using System.Buffers.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Spotta.Application.Membresias;
using Spotta.Domain.Accesos;
using Spotta.Domain.Usuarios;
using Spotta.Infrastructure;

namespace Spotta.Application.Usuarios;

public sealed record ListarUsuariosFiltro(string? Busqueda, string? Estado, string? Cursor, int? Limite);

public sealed record UsuarioResumen(
    Guid Id,
    string Nombre,
    string Apellidos,
    string DniParcial,
    string Email,
    Rol Rol,
    bool DeBaja,
    SituacionDeMembresia Membresia,
    DateTimeOffset? UltimoAcceso);

public sealed record PaginaDeUsuarios(IReadOnlyList<UsuarioResumen> Usuarios, string? Siguiente);

/// <summary>
/// El listado de recepción: búsqueda por nombre, apellidos o documento, filtro
/// por situación de membresía y paginación por cursor. El documento sale
/// enmascarado (RN-06).
/// </summary>
public sealed class ListarUsuarios(SpottaDb db, TimeProvider reloj)
{
    private const int LimitePorDefecto = 25;
    private const int LimiteMaximo = 100;

    public async Task<PaginaDeUsuarios> EjecutarAsync(ListarUsuariosFiltro filtro, Guid autorId, CancellationToken ct)
    {
        var autor = await PerfilActivo.CargarAsync(db, autorId, ct);
        var gimnasio = await db.Gimnasios.AsNoTracking().FirstAsync(g => g.Id == autor.GimnasioId, ct);
        var hoy = gimnasio.FechaLocal(reloj.GetUtcNow());
        var limite = Math.Clamp(filtro.Limite ?? LimitePorDefecto, 1, LimiteMaximo);

        var consulta = db.Perfiles.AsNoTracking().Where(p => p.GimnasioId == autor.GimnasioId);

        if (!string.IsNullOrWhiteSpace(filtro.Busqueda))
        {
            var patron = "%" + EscaparLike(filtro.Busqueda.Trim()) + "%";
            var documento = Documento.Normalizar(filtro.Busqueda);

            // Nombre y apellidos juntos, en los dos órdenes: en recepción se busca
            // «Adriano Paniagua» tanto como «Paniagua Adriano»
            consulta = consulta.Where(p =>
                EF.Functions.ILike(p.Nombre + " " + p.Apellidos, patron, "\\") ||
                EF.Functions.ILike(p.Apellidos + " " + p.Nombre, patron, "\\") ||
                p.Dni.Contains(documento));
        }

        if (!string.IsNullOrWhiteSpace(filtro.Estado))
            consulta = FiltrarPorMembresia(consulta, filtro.Estado, hoy);

        if (filtro.Cursor is not null)
        {
            var (apellidos, nombre, id) = LeerCursor(filtro.Cursor);
            consulta = consulta.Where(p =>
                string.Compare(p.Apellidos, apellidos) > 0 ||
                (p.Apellidos == apellidos && string.Compare(p.Nombre, nombre) > 0) ||
                (p.Apellidos == apellidos && p.Nombre == nombre && p.Id.CompareTo(id) > 0));
        }

        // Uno de más para saber si hay página siguiente sin contar la tabla
        var perfiles = await consulta
            .OrderBy(p => p.Apellidos).ThenBy(p => p.Nombre).ThenBy(p => p.Id)
            .Take(limite + 1)
            .ToListAsync(ct);

        var hayMas = perfiles.Count > limite;
        if (hayMas) perfiles.RemoveAt(limite);

        var ids = perfiles.Select(p => p.Id).ToList();
        var membresias = (await db.Membresias.AsNoTracking()
                .Where(m => ids.Contains(m.UsuarioId))
                .ToListAsync(ct))
            .ToLookup(m => m.UsuarioId);

        // Un intento rechazado en la puerta no es un acceso
        var ultimosAccesos = await db.Accesos.AsNoTracking()
            .Where(a => ids.Contains(a.UsuarioId) && a.Resultado == ResultadoAcceso.Permitido)
            .GroupBy(a => a.UsuarioId)
            .Select(g => new { UsuarioId = g.Key, Ultimo = g.Max(a => a.OcurridoEn) })
            .ToDictionaryAsync(x => x.UsuarioId, x => x.Ultimo, ct);

        var usuarios = perfiles
            .Select(p => new UsuarioResumen(
                p.Id, p.Nombre, p.Apellidos, Documento.Enmascarar(p.Dni), p.Email, p.Rol, p.EstaDeBaja,
                SituacionDeMembresia.De(membresias[p.Id], hoy),
                ultimosAccesos.TryGetValue(p.Id, out var ultimo) ? ultimo : null))
            .ToList();

        return new PaginaDeUsuarios(usuarios, hayMas ? EscribirCursor(perfiles[^1]) : null);
    }

    /// <summary>
    /// Las mismas reglas que <see cref="SituacionDeMembresia.De"/>, escritas en
    /// SQL: el filtro tiene que aplicarse antes de paginar, no después.
    /// </summary>
    private IQueryable<Perfil> FiltrarPorMembresia(IQueryable<Perfil> consulta, string estado, DateOnly hoy) =>
        estado switch
        {
            SituacionDeMembresia.Vigente => consulta.Where(p =>
                db.Membresias.Any(m => m.UsuarioId == p.Id && m.FechaInicio <= hoy && m.FechaFin >= hoy)),

            SituacionDeMembresia.Programada => consulta.Where(p =>
                !db.Membresias.Any(m => m.UsuarioId == p.Id && m.FechaInicio <= hoy && m.FechaFin >= hoy) &&
                db.Membresias.Any(m => m.UsuarioId == p.Id && m.FechaInicio > hoy)),

            SituacionDeMembresia.Vencida => consulta.Where(p =>
                db.Membresias.Any(m => m.UsuarioId == p.Id) &&
                !db.Membresias.Any(m => m.UsuarioId == p.Id && m.FechaFin >= hoy)),

            SituacionDeMembresia.SinMembresia => consulta.Where(p =>
                !db.Membresias.Any(m => m.UsuarioId == p.Id)),

            _ => throw ErroresUsuarios.EstadoDesconocido(estado),
        };

    private static string EscaparLike(string texto) =>
        texto.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");

    /// <summary>
    /// El cursor es la posición del último elemento en el orden del listado. Va
    /// codificado para que el cliente lo trate como opaco y no lo construya.
    /// </summary>
    private static string EscribirCursor(Perfil ultimo) =>
        Base64Url.EncodeToString(JsonSerializer.SerializeToUtf8Bytes(
            new[] { ultimo.Apellidos, ultimo.Nombre, ultimo.Id.ToString() }));

    private static (string Apellidos, string Nombre, Guid Id) LeerCursor(string cursor)
    {
        try
        {
            var partes = JsonSerializer.Deserialize<string[]>(Base64Url.DecodeFromChars(cursor));
            if (partes is [var apellidos, var nombre, var id] && Guid.TryParse(id, out var guid))
                return (apellidos, nombre, guid);
        }
        catch (Exception e) when (e is FormatException or JsonException)
        {
            // Cae al error de abajo
        }

        throw ErroresUsuarios.CursorInvalido();
    }
}
