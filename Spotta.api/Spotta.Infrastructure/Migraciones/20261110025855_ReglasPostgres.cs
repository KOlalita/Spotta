using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Spotta.Infrastructure.Migraciones
{
    /// <summary>
    /// Lo que el modelo de EF Core no sabe expresar y aun así es parte del esquema:
    /// la restricción de exclusión que impide membresías solapadas, la llave ajena
    /// contra <c>auth.users</c> de Supabase y la activación de RLS.
    /// </summary>
    public partial class ReglasPostgres : Migration
    {
        private static readonly string[] Tablas =
        [
            "gimnasio", "perfil", "plan_membresia", "membresia", "ajuste_membresia",
            "acceso", "pase_dispositivo", "pase_consumido", "notificacion",
            "notificacion_lectura", "dispositivo_push"
        ];

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Hace falta para combinar '=' sobre uuid con '&&' sobre un rango de
            // fechas dentro de la misma restricción de exclusión.
            //
            // Va en el esquema 'extensions', no en 'public': el linter de Supabase
            // avisa de las extensiones en public, porque ese esquema queda expuesto
            // a través de la API de datos. El SET search_path deja sus operadores
            // visibles para el ALTER TABLE de abajo, también en un Postgres pelado
            // donde 'extensions' no está en la ruta por defecto.
            migrationBuilder.Sql("CREATE SCHEMA IF NOT EXISTS extensions;");
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS btree_gist WITH SCHEMA extensions;");
            migrationBuilder.Sql("SET search_path = public, extensions;");

            // RN-13: ninguna membresía de un mismo usuario puede solaparse. La
            // validación en C# da el mensaje legible; la garantía está aquí, y es
            // lo único que sobrevive a dos recepcionistas vendiendo a la vez.
            migrationBuilder.Sql("""
                ALTER TABLE membresia
                ADD CONSTRAINT sin_solape
                EXCLUDE USING gist (
                    usuario_id WITH =,
                    daterange(fecha_inicio, fecha_fin, '[]') WITH &&
                );
                """);

            // El perfil es el espejo de auth.users y comparte su identificador.
            // En un Postgres pelado (desarrollo local sin Supabase) ese esquema no
            // existe, así que la llave sólo se crea donde tiene sentido.
            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM information_schema.schemata WHERE schema_name = 'auth') THEN
                        ALTER TABLE perfil
                        ADD CONSTRAINT fk_perfil_auth_users
                        FOREIGN KEY (id) REFERENCES auth.users(id) ON DELETE RESTRICT;
                    END IF;
                END $$;
                """);

            // RLS activado y sin una sola política: nadie pasa salvo el dueño de la
            // tabla, que es como se conecta la API. Toda la autorización real vive
            // en .NET; esto es la red por si algún día algo habla directo con
            // Postgres (PostgREST, la consola de Supabase, una clave filtrada).
            foreach (var tabla in Tablas)
            {
                migrationBuilder.Sql($"ALTER TABLE {tabla} ENABLE ROW LEVEL SECURITY;");
            }

            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'anon') THEN
                        REVOKE ALL ON ALL TABLES IN SCHEMA public FROM anon;
                    END IF;
                    IF EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'authenticated') THEN
                        REVOKE ALL ON ALL TABLES IN SCHEMA public FROM authenticated;
                    END IF;
                END $$;
                """);

            // El antirreplay sólo necesita retener los últimos minutos (RN-21).
            migrationBuilder.Sql("""
                CREATE INDEX ix_pase_consumido_limpieza
                ON pase_consumido (consumido_en);
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX IF EXISTS ix_pase_consumido_limpieza;");

            foreach (var tabla in Tablas)
            {
                migrationBuilder.Sql($"ALTER TABLE {tabla} DISABLE ROW LEVEL SECURITY;");
            }

            migrationBuilder.Sql("ALTER TABLE perfil DROP CONSTRAINT IF EXISTS fk_perfil_auth_users;");
            migrationBuilder.Sql("ALTER TABLE membresia DROP CONSTRAINT IF EXISTS sin_solape;");
        }
    }
}
