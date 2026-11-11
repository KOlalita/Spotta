using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Spotta.Infrastructure;

/// <summary>
/// Reconoce el choque contra un índice único concreto. Comprobar antes de
/// insertar da el mensaje claro; esto cubre la carrera de dos recepcionistas
/// dando de alta el mismo DNI en el mismo segundo, que sólo Postgres ve.
/// </summary>
public static class ViolacionDeUnicidad
{
    public static bool Es(Exception ex, string indice) =>
        ex is DbUpdateException { InnerException: PostgresException pg }
        && pg.SqlState == PostgresErrorCodes.UniqueViolation
        && pg.ConstraintName == indice;
}
