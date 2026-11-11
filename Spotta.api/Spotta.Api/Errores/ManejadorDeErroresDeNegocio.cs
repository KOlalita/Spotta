using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Spotta.Application.Comun;

namespace Spotta.Api.Errores;

/// <summary>
/// Convierte un <see cref="ErrorDeNegocio"/> en un ProblemDetails (RFC 7807)
/// con su <c>code</c> estable, que es lo que traducen los clientes
/// (arquitectura, sección 8). Cualquier otra excepción sigue su camino y
/// acaba en un 500.
/// </summary>
public sealed class ManejadorDeErroresDeNegocio(IProblemDetailsService problemas) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext http, Exception excepcion, CancellationToken ct)
    {
        if (excepcion is not ErrorDeNegocio error) return false;

        var estado = error.Tipo switch
        {
            TipoDeError.DatosInvalidos => StatusCodes.Status400BadRequest,
            TipoDeError.NoEncontrado => StatusCodes.Status404NotFound,
            TipoDeError.Conflicto => StatusCodes.Status409Conflict,
            TipoDeError.Prohibido => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status400BadRequest,
        };

        http.Response.StatusCode = estado;

        return await problemas.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = http,
            Exception = error,
            ProblemDetails = new ProblemDetails
            {
                Status = estado,
                Title = error.Message,
                Extensions = { ["code"] = error.Codigo },
            },
        });
    }
}
