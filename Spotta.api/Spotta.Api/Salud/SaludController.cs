using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Spotta.Infrastructure;

namespace Spotta.Api.Salud;

public sealed record SaludRespuesta(string Servicio, string Version, string BaseDeDatos, DateTimeOffset Ahora);

/// <summary>
/// Comprueba que la API responde y que alcanza la base de datos. Es el único
/// endpoint abierto: los clientes lo llaman antes de tener sesión.
/// </summary>
[ApiController]
[Route("api/salud")]
[AllowAnonymous]
public sealed class SaludController(SpottaDb db) : ControllerBase
{
    [HttpGet]
    public async Task<SaludRespuesta> Obtener(CancellationToken ct)
    {
        var baseDeDatos = await db.Database.CanConnectAsync(ct) ? "conectada" : "sin conexion";
        return new SaludRespuesta("Spotta.Api", "0.2.0", baseDeDatos, DateTimeOffset.UtcNow);
    }
}
