using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Spotta.Domain;
using Spotta.Infrastructure;

namespace Spotta.Api;

/// <summary>
/// Crea el primer administrador del gimnasio.
///
/// Hay un problema de arranque: el alta de usuarios exige un administrador
/// autenticado (RN-01), así que el primero no puede entrar por ahí. Este comando
/// es la única puerta de atrás, y usa la clave de servicio de Supabase, que vive
/// en user-secrets y nunca sale por pantalla.
///
///     dotnet run --project Spotta.Api -- sembrar-admin \
///         --email recepcion@spotta.pe --nombre Ana --apellidos Quispe --dni 70123456
///
/// El flujo es el mismo que implementará la Fase 1: crear en Supabase, insertar
/// el perfil y, si el perfil falla, deshacer el usuario. Un usuario en Supabase
/// sin perfil es un estado inválido.
/// </summary>
internal static class SembrarAdmin
{
    public static async Task<int> EjecutarAsync(string[] args)
    {
        var cfg = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)
            .AddUserSecrets(typeof(SembrarAdmin).Assembly, optional: true)
            .AddEnvironmentVariables()
            .Build();

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
        var cadena = cfg.GetConnectionString("Postgres");

        foreach (var (clave, valor) in new[]
                 {
                     ("Supabase:Url", urlSupabase),
                     ("Supabase:ServiceRoleKey", claveServicio),
                     ("ConnectionStrings:Postgres", cadena),
                 })
        {
            if (!string.IsNullOrWhiteSpace(valor)) continue;
            Console.Error.WriteLine($"  Falta la configuración {clave}.");
            Console.Error.WriteLine($"  dotnet user-secrets --project Spotta.Api set \"{clave}\" \"...\"");
            return 1;
        }

        await using var db = new SpottaDb(
            new DbContextOptionsBuilder<SpottaDb>().UseNpgsql(cadena).Options);

        var gimnasio = await db.Gimnasios.OrderBy(g => g.CreadoEn).FirstOrDefaultAsync();
        if (gimnasio is null)
        {
            Console.Error.WriteLine("  No hay ningún gimnasio en la base de datos. Siémbralo primero.");
            return 1;
        }

        if (await db.Perfiles.AnyAsync(p => p.Dni == o.Dni))
        {
            Console.Error.WriteLine($"  Ya existe un perfil con el documento {o.Dni}.");
            return 1;
        }

        using var http = new HttpClient { BaseAddress = new Uri(urlSupabase!) };
        http.DefaultRequestHeaders.Add("apikey", claveServicio);
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", claveServicio);

        // La contraseña inicial es el documento (RN-04)
        var respuesta = await http.PostAsJsonAsync("/auth/v1/admin/users", new
        {
            email = o.Email,
            password = o.Dni,
            email_confirm = true,
            app_metadata = new { role = "administrador" },
        });

        if (!respuesta.IsSuccessStatusCode)
        {
            Console.Error.WriteLine(
                $"  Supabase rechazó el alta ({(int)respuesta.StatusCode}): " +
                await respuesta.Content.ReadAsStringAsync());
            return 1;
        }

        var creado = JsonDocument.Parse(await respuesta.Content.ReadAsStringAsync());
        var id = Guid.Parse(creado.RootElement.GetProperty("id").GetString()!);

        try
        {
            db.Perfiles.Add(new Perfil
            {
                Id = id,
                GimnasioId = gimnasio.Id,
                Nombre = o.Nombre,
                Apellidos = o.Apellidos,
                Dni = o.Dni,
                Email = o.Email,
                Rol = Rol.Administrador,
                ContrasenaCambiada = false,
            });
            await db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // Compensación: sin perfil, ese usuario de Supabase no debe quedar vivo
            await http.DeleteAsync($"/auth/v1/admin/users/{id}");
            Console.Error.WriteLine($"  No se pudo crear el perfil, se deshizo el usuario: {ex.Message}");
            return 1;
        }

        Console.WriteLine($"  Administrador creado en {gimnasio.Nombre}.");
        Console.WriteLine($"  Correo:     {o.Email}");
        Console.WriteLine($"  Contraseña: su documento. Cámbiala al entrar (RN-05).");
        return 0;
    }

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

            var dni = Documento.Normalizar(pares["dni"]);
            if (!Documento.EsValido(dni))
            {
                queja = "El documento debe ser un DNI de 8 dígitos o un carné de extranjería de 9.";
                return false;
            }

            if (!pares["email"].Contains('@', StringComparison.Ordinal))
            {
                queja = "El correo no parece un correo.";
                return false;
            }

            opciones = new Opciones(pares["email"], pares["nombre"], pares["apellidos"], dni);
            queja = "";
            return true;
        }
    }
}
