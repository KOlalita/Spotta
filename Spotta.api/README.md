# Spotta.api

API en C# / .NET 10. Único punto por el que los clientes acceden a los datos:
ni el panel ni la aplicación móvil hablan con Postgres.

## Proyectos

Capas tradicionales: las dependencias bajan en línea recta y no hay inversión.

| Proyecto | Qué contiene | Referencia a |
|---|---|---|
| `Spotta.Api` | Controladores, DTO, CORS, arranque | las tres |
| `Spotta.Application` | Un archivo por caso de uso; consulta `SpottaDb` directamente | Domain, Infrastructure |
| `Spotta.Infrastructure` | `SpottaDb`, una configuración de EF por entidad, migraciones | Domain |
| `Spotta.Domain` | Entidades y enums, sin EF ni HTTP | nada |
| `Spotta.Tests` | Unitarios e integración | Domain, Infrastructure |

`Spotta.Domain` no referencia a nadie, ni un paquete: ahí viven las reglas que
hay que poder probar en milisegundos, como el cálculo de racha. No es
disciplina, es el compilador.

Dentro de cada proyecto, una carpeta por módulo funcional con los mismos
nombres que en `docs/modulos.md`: `Gimnasios`, `Socios`, `Membresias`,
`Accesos`, `Avisos`. **No hay repositorios ni interfaces sobre EF Core**; el
porqué está en `docs/arquitectura.md`, decisiones D8 y D8b.

## Arrancar

```bash
dotnet run --project Spotta.Api
```

Queda en `http://localhost:5240`. Comprobación rápida:

```bash
curl http://localhost:5240/api/salud
```

## Endpoints

| Método | Ruta | Rol | |
|---|---|---|---|
| `GET` | `/api/salud` | público | ¿responde la API y alcanza la base? |
| `GET` | `/api/me` | cliente | el perfil propio, con la situación de membresía |
| `GET` | `/api/usuarios` | administrador | `?q=` búsqueda, `?estado=` filtro, `?cursor=` página |
| `GET` | `/api/usuarios/{id}` | administrador | la ficha, con el documento completo |
| `POST` | `/api/usuarios` | administrador | alta; contraseña inicial = documento |
| `PUT` | `/api/usuarios/{id}` | administrador | nombre, apellidos, documento y correo |
| `POST` | `/api/usuarios/{id}/baja` | administrador | bloquea la cuenta; falla con membresía vigente |
| `POST` | `/api/usuarios/{id}/reactivar` | administrador | |

Todo lo que no es `/api/salud` exige sesión. Los errores salen como
ProblemDetails con un `code` estable; la lista completa está en
`docs/arquitectura.md`, sección 8.

## Configuración

Las credenciales no viven en el repositorio: van en
`Spotta.Api/appsettings.Development.json`, que está en `.gitignore`. Copia
`appsettings.Development.example.json`, renómbralo y rellena los dos huecos.

```json
{
  "Supabase": { "ServiceRoleKey": "sb_secret_..." },
  "Postgres": {
    "Host": "aws-1-sa-east-1.pooler.supabase.com",
    "Port": "5432",
    "Database": "postgres",
    "Username": "postgres.<project-ref>",
    "Password": "la contraseña de la base"
  }
}
```

**La contraseña va en su propio campo, nunca dentro de una cadena de conexión
completa.** `CadenaDePostgres.Desde()` la compone con
`NpgsqlConnectionStringBuilder`, que escapa cualquier carácter. Pegarla a mano
entre puntos y comas falla en cuanto lleva un `;` o se cuela un espacio en
`SSL Mode`, y el error que devuelve Postgres —`28P01 password authentication
failed`— no apunta a la causa real. Costó una tarde averiguarlo.

Si prefieres una cadena completa, `ConnectionStrings:Postgres` sigue teniendo
prioridad sobre la sección `Postgres`. Es la vía para producción, por variable
de entorno.

| Clave | Para qué |
|---|---|
| `Postgres:Host` / `Port` / `Database` / `Username` | destino de la conexión |
| `Postgres:Password` | **secreta**, valor crudo sin codificar |
| `ConnectionStrings:Postgres` | cadena completa; si existe, manda sobre lo anterior |
| `Supabase:Url` | proyecto de Supabase (en `appsettings.json`, no es secreto) |
| `Supabase:ServiceRoleKey` | **secreta**, alta de usuarios por la Admin API |
| `Cors:Origenes` | orígenes del navegador: el panel (5173) y la vista web del móvil (8081). La aplicación nativa no necesita CORS |

Sin configuración, la API arranca igual contra un Postgres local por defecto y
`/api/salud` responde `sin conexion` en vez de caerse.

### Conexión a Supabase: usa el pooler

La conexión directa (`db.<ref>.supabase.co`) **sólo tiene IPv6**. Desde una red
IPv4 no se alcanza y el error es un críptico `Failed to connect to [2600:...]`.
Hay que usar el *shared pooler* en modo sesión: puerto `5432`, usuario
`postgres.<project-ref>`, y un host que **no se puede componer a partir de la
región** — sácalo del botón **Connect** del panel, pestaña *Session pooler*.

El modo transacción (`6543`) no sirve aquí: rompe las sentencias preparadas.

## El primer administrador

Hay un problema de arranque: el alta de usuarios exige un administrador ya
autenticado (**RN-01**), así que el primero no puede entrar por ahí.

```bash
dotnet run --project Spotta.Api -- sembrar-admin --email recepcion@spotta.pe --nombre Ana --apellidos Quispe --dni 70123456
```

Su contraseña inicial es su documento (**RN-04**). Por dentro es exactamente el
mismo alta que usa el panel (`CrearUsuario`), sólo que sin autor: crea la cuenta
en Supabase con el rol en `app_metadata`, inserta el perfil y, si el perfil
falla, deshace la cuenta. Lee la clave de servicio de
`appsettings.Development.json`, así que no aparece en pantalla ni en el
historial del shell.

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

**El esquema va en snake_case.** El modelo en C# usa PascalCase y
`ConvencionesDelEsquema` traduce los nombres en dos pasadas. Postgres pliega a minúsculas cualquier
identificador sin comillas, así que `FechaInicio` acabaría como `fechainicio`.

**RLS está activado sin políticas.** No pasa nadie salvo el dueño de la tabla,
que es como se conecta la API. La autorización real vive en .NET; esto es la red
por si algo habla directo con Postgres.

**Puerto de Supabase.** El 6543 es el pooler en modo transacción y rompe las
sentencias preparadas: si alguna vez lo usas, añade
`No Reset On Close=true;Max Auto Prepare=0` a la cadena.

**Los avisos del panel de Supabase que se dejan como están.** Revisados el
2026-11-11; ninguno es un fallo:

- *RLS Enabled No Policy*, en las once tablas. Es a propósito: nadie pasa salvo
  la API, que se conecta como dueña de las tablas.
- *Leaked Password Protection Disabled*. Activarla rompería RN-04: rechaza las
  contraseñas que aparecen en filtraciones, y un número de 8 dígitos está en
  todas. El riesgo de que la contraseña inicial sea el DNI está asumido en la
  propia regla y se mitiga con RN-05 y RN-06.
- *Unused Index*, dieciocho. La base está casi vacía y ninguna consulta los ha
  necesitado todavía. Son los índices de las claves ajenas y los parciales del
  aforo y la racha, que entran en uso en las Fases 4 y 5. Vale la pena volver a
  mirarlo con datos reales, no antes.
