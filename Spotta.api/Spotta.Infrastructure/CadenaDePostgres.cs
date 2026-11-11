using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Spotta.Infrastructure;

/// <summary>
/// Arma la cadena de conexión a partir de la configuración. La contraseña vive
/// en su propio campo y nunca se pega dentro de una cadena con estructura: un
/// espacio de más o un ';' en el valor la rompen, y el error que se obtiene
/// («password authentication failed») no apunta a la causa.
/// <see cref="NpgsqlConnectionStringBuilder"/> escapa lo que haga falta.
/// </summary>
public static class CadenaDePostgres
{
    /// <summary>
    /// <c>ConnectionStrings:Postgres</c> manda si existe, que es como se
    /// configura en producción. Si no, se compone desde la sección
    /// <c>Postgres</c>.
    /// </summary>
    public static string Desde(IConfiguration cfg)
    {
        var completa = cfg.GetConnectionString("Postgres");
        if (!string.IsNullOrWhiteSpace(completa)) return completa;

        var pg = cfg.GetSection("Postgres");
        return new NpgsqlConnectionStringBuilder
        {
            Host = pg["Host"] ?? "localhost",
            Port = int.TryParse(pg["Port"], out var puerto) ? puerto : 5432,
            Database = pg["Database"] ?? "postgres",
            Username = pg["Username"] ?? "postgres",
            Password = pg["Password"] ?? "postgres",
            SslMode = SslMode.Require
        }.ConnectionString;
    }
}
