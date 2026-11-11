namespace Spotta.Application.Comun;

/// <summary>
/// Las operaciones que tocan Supabase y Postgres no comparten transacción. Se
/// hacen en ese orden —Supabase primero— y, si Postgres falla, se deshace lo de
/// Supabase. El orden no es indiferente: una cuenta bloqueada con el perfil
/// activo se arregla repitiendo la orden; una cuenta activa con el perfil dado
/// de baja deja entrar a alguien que no debería.
/// </summary>
public static class Compensacion
{
    /// <summary>
    /// Si deshacer también falla, no se traga: los dos sistemas quedaron
    /// desalineados y alguien tiene que saberlo.
    /// </summary>
    public static async Task DeshacerAsync(Func<Task> deshacer, Exception causa, string queQuedaMal)
    {
        try
        {
            await deshacer();
        }
        catch (Exception fallo)
        {
            throw new InvalidOperationException(
                $"Falló Postgres y tampoco se pudo deshacer el cambio en Supabase: {queQuedaMal}.",
                new AggregateException(causa, fallo));
        }
    }
}
