using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Spotta.Api.Autenticacion;
using Spotta.Application.Usuarios;
using Spotta.Domain.Usuarios;

namespace Spotta.Api.Usuarios;

/// <summary>
/// El socio consultándose a sí mismo (módulo M1). Sólo clientes: un
/// administrador que llame aquí recibe 403, que es justo lo que pide el
/// criterio de terminado de la Fase 1.
/// </summary>
[ApiController]
[Route("api/me")]
[Authorize(Roles = nameof(Rol.Cliente))]
public sealed class MiPerfilController : ControllerBase
{
    [HttpGet]
    public Task<MiPerfil> Obtener([FromServices] ObtenerMiPerfil casoDeUso, CancellationToken ct) =>
        casoDeUso.EjecutarAsync(User.Id(), ct);
}
