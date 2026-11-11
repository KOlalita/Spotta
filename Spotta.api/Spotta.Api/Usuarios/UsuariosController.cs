using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Spotta.Api.Autenticacion;
using Spotta.Application.Usuarios;
using Spotta.Domain.Usuarios;

namespace Spotta.Api.Usuarios;

/// <summary>
/// Gestión de usuarios desde recepción (módulo M2). Todo exige administrador:
/// no hay registro público (RN-01).
///
/// Cada acción recibe sólo el caso de uso que ejecuta y le pasa quién lo pide,
/// sacado del token. El controlador no decide nada: traduce HTTP.
/// </summary>
[ApiController]
[Route("api/usuarios")]
[Authorize(Roles = nameof(Rol.Administrador))]
public sealed class UsuariosController : ControllerBase
{
    [HttpGet]
    public Task<PaginaDeUsuarios> Listar(
        [FromServices] ListarUsuarios casoDeUso,
        [FromQuery(Name = "q")] string? busqueda,
        [FromQuery] string? estado,
        [FromQuery] string? cursor,
        [FromQuery] int? limite,
        CancellationToken ct) =>
        casoDeUso.EjecutarAsync(new ListarUsuariosFiltro(busqueda, estado, cursor, limite), User.Id(), ct);

    [HttpGet("{id:guid}")]
    public Task<FichaDeUsuario> Obtener(Guid id, [FromServices] ObtenerUsuario casoDeUso, CancellationToken ct) =>
        casoDeUso.EjecutarAsync(id, User.Id(), ct);

    [HttpPost]
    public async Task<ActionResult> Crear(
        CrearUsuarioSolicitud solicitud, [FromServices] CrearUsuario casoDeUso, CancellationToken ct)
    {
        var id = await casoDeUso.EjecutarAsync(solicitud, User.Id(), ct);
        return CreatedAtAction(nameof(Obtener), new { id }, new { id });
    }

    [HttpPut("{id:guid}")]
    public async Task<NoContentResult> Editar(
        Guid id, EditarUsuarioSolicitud solicitud, [FromServices] EditarUsuario casoDeUso, CancellationToken ct)
    {
        await casoDeUso.EjecutarAsync(id, solicitud, User.Id(), ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/baja")]
    public async Task<NoContentResult> DarDeBaja(
        Guid id, [FromServices] DarDeBajaUsuario casoDeUso, CancellationToken ct)
    {
        await casoDeUso.EjecutarAsync(id, User.Id(), ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/reactivar")]
    public async Task<NoContentResult> Reactivar(
        Guid id, [FromServices] ReactivarUsuario casoDeUso, CancellationToken ct)
    {
        await casoDeUso.EjecutarAsync(id, User.Id(), ct);
        return NoContent();
    }
}
