# Spotta.api

API en C# / .NET 10. Único punto por el que los clientes acceden a los datos:
ni el panel ni la aplicación móvil hablan con Postgres.

## Proyectos

| Proyecto | Qué contiene |
|---|---|
| `Spotta.Api` | Minimal APIs, autenticación, arranque |
| `Spotta.Domain` | Entidades y reglas puras, sin EF ni HTTP |
| `Spotta.Infrastructure` | `SpottaDb`, migraciones, acceso a Postgres |
| `Spotta.Tests` | Unitarios e integración |

`Spotta.Domain` no referencia a nadie: ahí viven las reglas que hay que poder
probar en milisegundos, como el cálculo de racha.

## Arrancar

```bash
dotnet run --project Spotta.Api
```

Queda en `http://localhost:5240`. Comprobación rápida:

```bash
curl http://localhost:5240/api/salud
```

## Configuración

La cadena de conexión no vive en el repositorio. En desarrollo:

```bash
dotnet user-secrets --project Spotta.Api set "ConnectionStrings:Postgres" "Host=...;Database=postgres;Username=...;Password=..."
```

Sin cadena configurada, la API arranca igual contra un Postgres local por
defecto y `/api/salud` responde `sin conexion` en vez de caerse.

| Variable | Para qué |
|---|---|
| `ConnectionStrings:Postgres` | conexión Npgsql |
| `Supabase:Url` | proyecto de Supabase (en `appsettings.json`, no es secreto) |
| `Supabase:ServiceRoleKey` | alta de usuarios por la Admin API. **Sólo en user-secrets** |
| `Cors:Origenes` | orígenes del panel web (el móvil no necesita CORS) |

## El primer administrador

Hay un problema de arranque: el alta de usuarios exige un administrador ya
autenticado (**RN-01**), así que el primero no puede entrar por ahí.

```bash
dotnet run --project Spotta.Api -- sembrar-admin --email recepcion@spotta.pe --nombre Ana --apellidos Quispe --dni 70123456
```

Su contraseña inicial es su documento (**RN-04**). El comando sigue el mismo
flujo que implementará la Fase 1: crea el usuario en Supabase con el rol en
`app_metadata`, inserta el perfil y, si el perfil falla, deshace el usuario.
Lee la clave de servicio de user-secrets, así que la clave no aparece en
pantalla ni en el historial del shell.

## Migraciones

```bash
dotnet ef migrations add <Nombre> --project Spotta.Infrastructure --startup-project Spotta.Api --output-dir Migraciones
dotnet ef database update --project Spotta.Infrastructure --startup-project Spotta.Api
```

Para ver el SQL sin tocar ninguna base de datos:

```bash
dotnet ef migrations script --project Spotta.Infrastructure --startup-project Spotta.Api --idempotent
```

Hay dos migraciones:

- **`Inicial`** — las once tablas, generadas desde el modelo.
- **`ReglasPostgres`** — lo que EF Core no sabe expresar: la extensión
  `btree_gist`, la restricción de exclusión que impide membresías solapadas, la
  llave ajena contra `auth.users` de Supabase y la activación de RLS.

La llave contra `auth.users` sólo se crea si el esquema `auth` existe, así que
las migraciones corren igual sobre un Postgres pelado.

## Detalles que ahorran una tarde

**El esquema va en snake_case.** El modelo en C# usa PascalCase y una convención
en `SpottaDb` traduce los nombres. Postgres pliega a minúsculas cualquier
identificador sin comillas, así que `FechaInicio` acabaría como `fechainicio`.

**RLS está activado sin políticas.** No pasa nadie salvo el dueño de la tabla,
que es como se conecta la API. La autorización real vive en .NET; esto es la red
por si algo habla directo con Postgres.

**Puerto de Supabase.** El 5432 es Postgres directo. El 6543 es el pooler en
modo transacción y rompe las sentencias preparadas: si lo usas, añade
`No Reset On Close=true;Max Auto Prepare=0` a la cadena.
