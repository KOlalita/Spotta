using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Spotta.Domain.Usuarios;

namespace Spotta.Infrastructure.Supabase;

/// <summary>
/// Las cuentas de Supabase Auth, vistas desde la API de administración. Es la
/// única pieza que usa la clave de servicio, que se salta RLS entera: por eso
/// vive en el servidor y nunca llega a un cliente.
/// </summary>
public sealed class ClienteAuthSupabase(HttpClient http)
{
    /// <summary>Cien años: Supabase no tiene «para siempre», tiene duraciones.</summary>
    private const string BloqueoIndefinido = "876000h";

    public static void Configurar(HttpClient http, string urlProyecto, string claveDeServicio)
    {
        http.BaseAddress = new Uri(urlProyecto);
        http.DefaultRequestHeaders.Add("apikey", claveDeServicio);
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", claveDeServicio);
    }

    /// <summary>
    /// El correo se da por confirmado: quien crea la cuenta es recepción, con el
    /// socio delante (RN-01). El rol va en <c>app_metadata</c>, que el usuario no
    /// puede modificar, y de ahí sale en cada token (RN-07).
    /// </summary>
    public async Task<Guid> CrearUsuarioAsync(string email, string contrasena, Rol rol, CancellationToken ct)
    {
        var respuesta = await http.PostAsJsonAsync("/auth/v1/admin/users", new
        {
            email,
            password = contrasena,
            email_confirm = true,
            app_metadata = new { role = rol.ToString().ToLowerInvariant() },
        }, ct);

        await AsegurarExitoAsync(respuesta, ct);

        using var json = JsonDocument.Parse(await respuesta.Content.ReadAsStringAsync(ct));
        return Guid.Parse(json.RootElement.GetProperty("id").GetString()!);
    }

    public Task CambiarContrasenaAsync(Guid id, string contrasena, CancellationToken ct) =>
        ActualizarAsync(id, new { password = contrasena }, ct);

    /// <summary>
    /// Comprueba una contraseña intentando iniciar sesión con ella. Es la única
    /// forma que ofrece Supabase, y crea una sesión que no se usa: caduca sola.
    /// </summary>
    public async Task<bool> ComprobarContrasenaAsync(string email, string contrasena, CancellationToken ct)
    {
        var respuesta = await http.PostAsJsonAsync(
            "/auth/v1/token?grant_type=password", new { email, password = contrasena }, ct);

        if (respuesta.IsSuccessStatusCode) return true;
        if ((int)respuesta.StatusCode == 400) return false;

        await AsegurarExitoAsync(respuesta, ct);
        return false;
    }

    public Task CambiarEmailAsync(Guid id, string email, CancellationToken ct) =>
        ActualizarAsync(id, new { email, email_confirm = true }, ct);

    /// <summary>
    /// Impide iniciar sesión y renovar el token. El token que ya tenga en la
    /// mano sigue siendo válido hasta que caduque; eso lo cubre la API
    /// comprobando la baja en cada petición.
    /// </summary>
    public Task BloquearAsync(Guid id, CancellationToken ct) =>
        ActualizarAsync(id, new { ban_duration = BloqueoIndefinido }, ct);

    public Task DesbloquearAsync(Guid id, CancellationToken ct) =>
        ActualizarAsync(id, new { ban_duration = "none" }, ct);

    public async Task BorrarUsuarioAsync(Guid id, CancellationToken ct)
    {
        var respuesta = await http.DeleteAsync($"/auth/v1/admin/users/{id}", ct);
        await AsegurarExitoAsync(respuesta, ct);
    }

    private async Task ActualizarAsync(Guid id, object cambios, CancellationToken ct)
    {
        var respuesta = await http.PutAsJsonAsync($"/auth/v1/admin/users/{id}", cambios, ct);
        await AsegurarExitoAsync(respuesta, ct);
    }

    private static async Task AsegurarExitoAsync(HttpResponseMessage respuesta, CancellationToken ct)
    {
        if (respuesta.IsSuccessStatusCode) return;

        var cuerpo = await respuesta.Content.ReadAsStringAsync(ct);
        string? codigo = null;
        try
        {
            using var json = JsonDocument.Parse(cuerpo);
            if (json.RootElement.TryGetProperty("error_code", out var c)) codigo = c.GetString();
        }
        catch (JsonException)
        {
            // Cuerpo que no es JSON: se conserva entero en el mensaje
        }

        throw new ErrorDeSupabase((int)respuesta.StatusCode, codigo, cuerpo);
    }
}
