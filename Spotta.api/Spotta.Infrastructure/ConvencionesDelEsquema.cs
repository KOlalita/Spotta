using System.Text;
using Microsoft.EntityFrameworkCore;
using Spotta.Domain.Accesos;
using Spotta.Domain.Notificaciones;

namespace Spotta.Infrastructure;

/// <summary>
/// Las dos reglas que no son de una entidad sino del esquema entero, y que por
/// eso no caben en ninguna clase de configuración.
/// </summary>
internal static class ConvencionesDelEsquema
{
    /// <summary>
    /// Aquí no se borra nada (RN-08): un usuario se da de baja, un plan se
    /// archiva. EF pone cascada por defecto en las relaciones obligatorias, y eso
    /// convierte un borrado accidental en la pérdida silenciosa del historial de
    /// accesos y cobros. Mejor que falle y se note.
    ///
    /// Las dos excepciones son tablas hijas sin valor propio: el antirreplay y
    /// las marcas de lectura.
    /// </summary>
    public static void ProhibirBorradoEnCascada(ModelBuilder b)
    {
        Type[] puedenCaerConSuPadre = [typeof(PaseConsumido), typeof(NotificacionLectura)];

        foreach (var entidad in b.Model.GetEntityTypes())
        {
            if (puedenCaerConSuPadre.Contains(entidad.ClrType)) continue;

            foreach (var ajena in entidad.GetForeignKeys())
                ajena.DeleteBehavior = DeleteBehavior.Restrict;
        }
    }

    /// <summary>
    /// Postgres pliega a minúsculas cualquier identificador sin comillas, así que
    /// <c>FechaInicio</c> acabaría como <c>fechainicio</c> y habría que entrecomillar
    /// en cada consulta a mano. Con esto, el modelo en C# va en PascalCase y el
    /// esquema en snake_case, sin anotar una sola columna.
    /// </summary>
    public static void AplicarNombresEnMinusculaConGuiones(ModelBuilder b)
    {
        // Primera pasada: tablas y columnas.
        foreach (var entidad in b.Model.GetEntityTypes())
        {
            entidad.SetTableName(ConGuiones(entidad.ClrType.Name));

            foreach (var propiedad in entidad.GetProperties())
                propiedad.SetColumnName(ConGuiones(propiedad.Name));
        }

        // Segunda pasada: los nombres que EF deriva de los anteriores. Separarla
        // no es cosmética. EF compone el nombre de una clave ajena con el de la
        // tabla destino, así que en un solo bucle el resultado dependería de si
        // esa tabla ya pasó por él: "fk_notificacion_gimnasio_gimnasio_id" si ya
        // pasó, "fk_notificacion_gimnasios_gimnasio_id" si no. El orden de
        // GetEntityTypes() no está garantizado, con lo que el esquema salía
        // distinto según cómo se hubieran registrado las configuraciones.
        foreach (var entidad in b.Model.GetEntityTypes())
        {
            // Los nombres que pone EF ya vienen separados ("PK_perfil",
            // "IX_acceso_usuario_id"): sólo hay que bajarlos a minúsculas.
            // Pasarlos por ConGuiones los dejaría como "i_x_…".
            foreach (var clave in entidad.GetKeys())
                clave.SetName(clave.GetName()!.ToLowerInvariant());

            foreach (var ajena in entidad.GetForeignKeys())
                ajena.SetConstraintName(ajena.GetConstraintName()!.ToLowerInvariant());

            foreach (var indice in entidad.GetIndexes())
                indice.SetDatabaseName(indice.GetDatabaseName()!.ToLowerInvariant());
        }
    }

    private static string ConGuiones(string nombre)
    {
        var sb = new StringBuilder(nombre.Length + 8);
        for (var i = 0; i < nombre.Length; i++)
        {
            var c = nombre[i];
            if (char.IsUpper(c))
            {
                if (i > 0 && nombre[i - 1] != '_') sb.Append('_');
                sb.Append(char.ToLowerInvariant(c));
            }
            else sb.Append(c);
        }
        return sb.ToString();
    }
}
