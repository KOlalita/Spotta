using Microsoft.Extensions.DependencyInjection;
using Spotta.Application.Usuarios;

namespace Spotta.Application;

/// <summary>
/// Cada caso de uso es una clase concreta con sus dependencias en el
/// constructor. Se registran uno a uno y a la vista, en vez de escanear el
/// ensamblado: así esta lista es también el índice de lo que la API sabe hacer.
/// </summary>
public static class RegistroDeServicios
{
    public static IServiceCollection AddCasosDeUso(this IServiceCollection servicios) => servicios
        .AddScoped<ListarUsuarios>()
        .AddScoped<ObtenerUsuario>()
        .AddScoped<CrearUsuario>()
        .AddScoped<EditarUsuario>()
        .AddScoped<DarDeBajaUsuario>()
        .AddScoped<ReactivarUsuario>()
        .AddScoped<ObtenerMiPerfil>();
}
