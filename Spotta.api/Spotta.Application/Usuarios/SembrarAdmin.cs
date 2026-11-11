using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Spotta.Application.Comun;
using Spotta.Domain.Usuarios;
using Spotta.Infrastructure;
using Spotta.Infrastructure.Supabase;

namespace Spotta.Application.Usuarios;

/// <summary>
/// Crea el primer administrador del gimnasio.
///
/// Hay un problema de arranque: el alta de usuarios exige un administrador
/// autenticado (RN-01), así que el primero no puede entrar por ahí. Este comando
/// es la única puerta de atrás. Por dentro es el mismo alta que usa el panel
/// (<see cref="CrearUsuario"/>), sólo que sin autor.
///
///     dotnet run --project Spotta.Api -- sembrar-admin \
///         --email recepcion@spotta.pe --nombre Ana --apellidos Quispe --dni 70123456
/// </summary>
public static class SembrarAdmin
{
    public static async Task<int> EjecutarAsync(string[] args, IConfiguration cfg)
    {
        if (!Opciones.Intentar(args, out var o, out var queja))
        {
            Console.Error.WriteLine($"  {queja}");
            Console.Error.WriteLine(
                "  uso: sembrar-admin --email <correo> --nombre <nombre> " +
                "--apellidos <apellidos> --dni <documento>");
            return 1;
        }

        var urlSupabase = cfg["Supabase:Url"];
        var claveServicio = cfg["Supabase:ServiceRoleKey"];

        foreach (var (clave, valor) in new[]
                 {
                     ("Supabase:Url", urlSupabase),
                     ("Supabase:ServiceRoleKey", claveServicio),
                 })
        {
            if (!string.IsNullOrWhiteSpace(valor)) continue;
            Console.Error.WriteLine($"  Falta {clave} en Spotta.Api/appsettings.Development.json.");
            return 1;
        }

        await using var db = new SpottaDb(
            new DbContextOptionsBuilder<SpottaDb>().UseNpgsql(CadenaDePostgres.Desde(cfg)).Options);

        var gimnasio = await db.Gimnasios.OrderBy(g => g.CreadoEn).FirstOrDefaultAsync();
        if (gimnasio is null)
        {
            Console.Error.WriteLine("  No hay ningún gimnasio en la base de datos. Siémbralo primero.");
            return 1;
        }

        using var http = new HttpClient();
        ClienteAuthSupabase.Configurar(http, urlSupabase!, claveServicio!);
        var alta = new CrearUsuario(db, new ClienteAuthSupabase(http));

        try
        {
            await alta.CrearEnGimnasioAsync(
                new CrearUsuarioSolicitud(o.Nombre, o.Apellidos, o.Dni, o.Email, Rol.Administrador),
                gimnasio.Id, autorId: null, CancellationToken.None);
        }
        catch (ErrorDeNegocio e)
        {
            Console.Error.WriteLine($"  {e.Message}");
            return 1;
        }

        Console.WriteLine($"  Administrador creado en {gimnasio.Nombre}.");
        Console.WriteLine($"  Correo:     {o.Email}");
        Console.WriteLine("  Contraseña: su documento. Cámbiala al entrar (RN-05).");
        return 0;
    }

    /// <summary>
    /// Sólo comprueba que estén los cuatro argumentos. Validarlos es cosa de
    /// <see cref="CrearUsuario"/>, igual que cuando el alta llega por el panel.
    /// </summary>
    private sealed record Opciones(string Email, string Nombre, string Apellidos, string Dni)
    {
        public static bool Intentar(string[] args, out Opciones opciones, out string queja)
        {
            opciones = new Opciones("", "", "", "");

            var pares = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i + 1 < args.Length; i += 2)
            {
                if (!args[i].StartsWith("--", StringComparison.Ordinal))
                {
                    queja = $"No entiendo el argumento «{args[i]}».";
                    return false;
                }
                pares[args[i][2..]] = args[i + 1];
            }

            foreach (var requerido in new[] { "email", "nombre", "apellidos", "dni" })
            {
                if (pares.ContainsKey(requerido)) continue;
                queja = $"Falta --{requerido}.";
                return false;
            }

            opciones = new Opciones(pares["email"], pares["nombre"], pares["apellidos"], pares["dni"]);
            queja = "";
            return true;
        }
    }
}
