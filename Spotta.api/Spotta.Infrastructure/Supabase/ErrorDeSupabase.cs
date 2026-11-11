namespace Spotta.Infrastructure.Supabase;

/// <summary>
/// Supabase rechazó una operación. <see cref="Codigo"/> es su <c>error_code</c>
/// (por ejemplo <c>email_exists</c>), que es lo que la capa de aplicación
/// traduce a un error de negocio. Aquí no se interpreta: esta capa no sabe qué
/// significa para Spotta que un correo esté repetido.
/// </summary>
public sealed class ErrorDeSupabase(int estado, string? codigo, string cuerpo)
    : Exception($"Supabase respondió {estado} ({codigo ?? "sin código"}): {cuerpo}")
{
    public int Estado { get; } = estado;
    public string? Codigo { get; } = codigo;
}
