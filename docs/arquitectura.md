# Spotta — Arquitectura

Decisiones técnicas, modelo de datos y flujos. Las reglas funcionales que
sustentan estas decisiones están en [reglas-de-negocio.md](reglas-de-negocio.md)
y se citan como `RN-xx`.

---

## 1. Panorama

```
┌──────────────────┐         ┌──────────────────┐
│  Spotta.mobile   │         │   Spotta.web     │
│  React Native    │         │  React + Vite    │
│  Expo  · socios  │         │  recepción/admin │
└────────┬─────────┘         └────────┬─────────┘
         │  HTTPS + JWT               │
         └─────────────┬──────────────┘
                       ▼
            ┌─────────────────────┐
            │     Spotta.api      │
            │  C# / .NET 10       │
            │  Minimal APIs + EF  │
            └──────┬───────┬──────┘
                   │       │
       Npgsql ─────┘       └───── Admin API / JWKS
                   ▼                      ▼
        ┌────────────────────┐   ┌──────────────────┐
        │ PostgreSQL         │   │  Supabase Auth   │
        │ (Supabase)         │   │  identidad       │
        └────────────────────┘   └──────────────────┘
                   ▲
                   │  Expo Push API
            (notificaciones salientes)
```

**Un único punto de verdad: la API.** Ni la web ni el móvil hablan directamente
con la base de datos. Supabase se usa como servicio de identidad y como Postgres
gestionado, no como backend.

---

## 2. Estructura del repositorio

```
Spotta/
├── docs/                      documentación viva
├── Spotta.api/
│   ├── Spotta.Api/            Minimal APIs, endpoints, filtros
│   ├── Spotta.Domain/         entidades, reglas, valores (DNI, rangos)
│   ├── Spotta.Infrastructure/ EF Core, Npgsql, Supabase, Expo Push
│   └── Spotta.Tests/          unitarios + integración
├── Spotta.web/                React + Vite + TypeScript
└── Spotta.mobile/             Expo + React Native + TypeScript
```

La API en tres proyectos porque las reglas de membresía y racha son el valor
real del sistema y merecen vivir sin dependencias de EF ni de HTTP, donde se
pueden probar en milisegundos.

---

## 3. Identidad y autorización

**Supabase Auth es el único proveedor de identidad.** No hay tabla de contraseñas
propia ni emisión de sesiones en la API.

### Alta de usuario (RN-01, RN-04)

1. Recepción envía `POST /api/usuarios` con el token de un administrador.
2. La API valida el DNI (**RN-03**) y comprueba que no exista.
3. La API llama a la Admin API de Supabase con la clave `service_role`
   (sólo en el servidor, nunca en un cliente) y crea el usuario con
   `email_confirm: true` y contraseña igual al DNI normalizado.
4. El rol se escribe en `app_metadata.role`, de modo que viaje dentro del JWT.
5. La API inserta el `perfil` con el mismo `id` que `auth.users.id`.

Los pasos 3 y 4 no son transaccionales con el 5. Si falla la inserción del
perfil, la API elimina el usuario recién creado en Supabase (compensación) y
devuelve error. Un usuario en Supabase sin perfil es un estado inválido.

### Validación de las peticiones

La API valida el JWT contra el JWKS público del proyecto de Supabase: no
comparte secretos con el cliente y no necesita llamar a Supabase en cada
petición. De los claims saca `sub` (el id del usuario) y `role`.

Dos políticas de autorización: `Administrador` y `Cliente`. Los endpoints de
gestión exigen la primera; los de `/api/me/*` exigen la segunda y resuelven el
usuario desde el token, nunca desde un parámetro de la URL.

### Fila cero de datos (RLS)

Todo el tráfico pasa por la API, así que la autorización real vive en .NET. Aun
así, **RLS activado y denegando por defecto en todas las tablas de negocio**. La
API se conecta con un rol propio que la política permite. Es la red de seguridad
para el día en que algo hable directo con Postgres.

### Rotación de la clave de servicio

La `service_role` de Supabase y la clave con la que se cifran los secretos de
pase viven en variables de entorno, nunca en el repositorio. En desarrollo, en
`dotnet user-secrets`.

---

## 4. El pase: TOTP calculado en el dispositivo

Es la pieza con más superficie de ataque del sistema, así que va explícita.

### El problema que resuelve

Un gimnasio suele estar en un sótano o una entreplanta, donde el móvil no tiene
cobertura. Si el pase hubiera que pedirlo al servidor en la puerta, no
funcionaría precisamente donde se usa.

La asimetría que lo salva: **el móvil puede estar sin conexión, el puesto de
recepción no.** La validación ocurre siempre en el servidor, llamada desde
recepción, que está en banda ancha fija. Así que sólo la **generación** tiene que
funcionar sin red.

### Enrolamiento (una vez, con conexión)

```
POST /api/me/pase                  (rol Cliente)
→ { dispositivoRef: "...", secreto: "...", paso: 30, algoritmo: "HMAC-SHA256" }
```

Ocurre al iniciar sesión, en el mostrador, delante de quien acaba de crear la
cuenta. El secreto **se devuelve una sola vez**: después sólo existe cifrado en
la base de datos y en el almacén seguro del dispositivo (Keychain en iOS,
Keystore en Android, vía `expo-secure-store`).

Cada dispositivo tiene su propio secreto y su propia referencia opaca, que no es
el identificador del usuario ni su DNI. Un socio puede tener varios dispositivos
activos (**RN-44**).

### Generación (siempre sin red)

La aplicación deriva el código con TOTP sobre su secreto y la hora corregida, y
lo renderiza como QR:

```
spotta:v1:<dispositivoRef>:<codigo>
```

Unos cuarenta caracteres: el QR sale pequeño, de módulos grandes, y lo lee sin
esfuerzo una cámara de portátil a medio metro.

**La hora corregida** es la del dispositivo más el desfase con el servidor
(**RN-45**), que la aplicación guarda en cada respuesta HTTP que recibe — la
cabecera `Date` basta — y conserva entre sesiones.

### Validación (en recepción, con red)

```
POST /api/check-ins   { pase }     (rol Administrador)
→ 200 { resultado, motivo, usuario: { nombre, apellidos, dniParcial },
        membresia: { estado, fechaFin, diasRestantes }, aforo: { actual, maximo } }
```

Orden (**RN-22**): formato → dispositivo conocido y no revocado → código
coincidente con tolerancia de ±1 paso → código no consumido → usuario no de baja
→ membresía vigente. El primer fallo determina el motivo del rechazo.

La comparación del código se hace en **tiempo constante**. Comparar cadenas con
`==` filtra información por el tiempo de respuesta, y aquí el atacante puede
repetir el intento tantas veces como quiera.

El par `(dispositivo, paso)` consumido se guarda en `pase_consumido`, que sólo
necesita retener los últimos minutos; una tarea de limpieza borra lo anterior a
una hora. Sin esa tabla, un QR fotografiado se podría reutilizar durante su medio
minuto de vida.

### Qué se gana y qué se paga

Se gana que el pase funcione sin cobertura, que es la diferencia entre un
producto y una demostración.

Se paga que el dispositivo guarde un secreto. Con un teléfono rooteado y acceso
al almacén seguro, alguien podría extraerlo y generar códigos válidos hasta que
se revoque el dispositivo. Se acota con secreto por dispositivo, revocación desde
el panel, revocación al cerrar sesión, y cifrado del secreto en la base de datos
con una clave que vive en el entorno y no en Postgres.

Es un riesgo de otro orden que el del QR estático, donde bastaba una captura de
pantalla reenviada por mensajería.

**Vía manual** (**RN-26**): `POST /api/check-ins/manual { usuarioId }`, mismo
flujo saltando las cuatro primeras comprobaciones y marcando el acceso como
manual.

---

## 5. Modelo de datos

Postgres. Claves primarias `uuid` generadas en la base de datos. Todas las marcas
de tiempo en `timestamptz` (UTC en almacenamiento; la conversión a la zona del
gimnasio ocurre al calcular días y semanas, nunca antes).

Este bloque refleja el esquema aplicado. La fuente de verdad son las migraciones
de `Spotta.Infrastructure/Migraciones`; si alguna vez discrepan, manda la
migración.

**Las llaves ajenas van en `on delete restrict`**, salvo las de
`pase_consumido` y `notificacion_lectura`, que son tablas hijas sin valor
propio. Aquí no se borra nada (**RN-08**): un borrado accidental debe fallar, no
llevarse por delante el historial de accesos y cobros.

**Los valores por defecto de la configuración viven en la entidad de C#, no en
la columna.** `America/Lima`, `PEN`, 60 minutos y 3 días son lo que trae un
`Gimnasio` recién construido en código; la columna es `not null` y sin
`default`, porque todas las escrituras pasan por la API y un valor por defecto
en la base sólo serviría para tapar un error de la capa de arriba.

```sql
-- En el esquema 'extensions', no en 'public': el linter de Supabase avisa de las
-- extensiones en public, que queda expuesto por la API de datos.
create schema if not exists extensions;
create extension if not exists btree_gist with schema extensions;

-- Configuración del gimnasio (RN-28, RN-29, RN-32, RN-42, RN-43)
create table gimnasio (
  id                  uuid primary key default gen_random_uuid(),
  nombre              text        not null,
  zona_horaria        text        not null,
  moneda              varchar(3)  not null,
  aforo_maximo        int         not null check (aforo_maximo > 0),
  ventana_presencia_min int       not null check (ventana_presencia_min between 15 and 480),
  dias_minimos_racha  int         not null check (dias_minimos_racha between 1 and 7),
  creado_en           timestamptz not null default now()
);

-- Perfil, espejo de auth.users (RN-02, RN-03, RN-07, RN-08, RN-09)
create table perfil (
  id                  uuid primary key references auth.users(id) on delete restrict,
  gimnasio_id         uuid        not null references gimnasio(id),
  nombre              text        not null,
  apellidos           text        not null,
  dni                 text        not null,
  email               text        not null,
  rol                 text        not null check (rol in ('administrador','cliente')),
  contrasena_cambiada boolean     not null default false,
  dado_de_baja_en     timestamptz null,
  dado_de_baja_por    uuid        null references perfil(id),
  creado_en           timestamptz not null default now(),
  creado_por          uuid        null references perfil(id),
  constraint dni_unico_por_gimnasio unique (gimnasio_id, dni)
);

-- Catálogo de planes (RN-10, RN-11, RN-12)
create table plan_membresia (
  id             uuid primary key default gen_random_uuid(),
  gimnasio_id    uuid        not null references gimnasio(id),
  nombre         text        not null,
  duracion_dias  int         not null check (duracion_dias > 0),
  precio         numeric(10,2) not null check (precio >= 0),
  archivado_en   timestamptz null,
  creado_en      timestamptz not null default now()
);

-- Membresía vendida, con cobro incorporado (RN-13..RN-20)
create table membresia (
  id                uuid primary key default gen_random_uuid(),
  gimnasio_id       uuid        not null references gimnasio(id),
  usuario_id        uuid        not null references perfil(id),
  plan_id           uuid        not null references plan_membresia(id),
  -- copia del plan en el momento de la venta (RN-11)
  plan_nombre       text        not null,
  duracion_dias     int         not null check (duracion_dias > 0),
  fecha_inicio      date        not null,
  fecha_fin         date        not null,
  -- cobro (RN-19, RN-20)
  precio_cobrado    numeric(10,2) not null check (precio_cobrado >= 0),
  metodo_pago       text        not null check (metodo_pago in ('efectivo','tarjeta','transferencia')),
  cobrado_en        timestamptz not null default now(),
  cobrado_por       uuid        not null references perfil(id),
  creado_en         timestamptz not null default now(),
  constraint periodo_valido check (fecha_fin >= fecha_inicio),
  -- RN-13: ninguna membresía de un mismo usuario puede solaparse
  constraint sin_solape exclude using gist (
    usuario_id with =,
    daterange(fecha_inicio, fecha_fin, '[]') with &&
  )
);

-- Ajustes de días, auditados (RN-17)
create table ajuste_membresia (
  id            uuid primary key default gen_random_uuid(),
  membresia_id  uuid        not null references membresia(id),
  dias          int         not null check (dias <> 0),
  motivo        text        not null check (length(trim(motivo)) > 0),
  creado_en     timestamptz not null default now(),
  creado_por    uuid        not null references perfil(id)
);

-- Accesos (RN-23, RN-25, RN-26, RN-27)
create table acceso (
  id            uuid primary key default gen_random_uuid(),
  gimnasio_id   uuid        not null references gimnasio(id),
  usuario_id    uuid        not null references perfil(id),
  membresia_id  uuid        null references membresia(id),
  ocurrido_en   timestamptz not null default now(),
  resultado     text        not null check (resultado in ('permitido','rechazado')),
  motivo        text        null,
  manual        boolean     not null default false,
  registrado_por uuid       null references perfil(id)
);
create index acceso_aforo_idx on acceso (gimnasio_id, ocurrido_en desc)
  where resultado = 'permitido';
create index acceso_usuario_idx on acceso (usuario_id, ocurrido_en desc)
  where resultado = 'permitido';

-- Dispositivos con pase (RN-44)
create table pase_dispositivo (
  id            uuid primary key default gen_random_uuid(),
  usuario_id    uuid        not null references perfil(id),
  secreto_cifrado bytea     not null,
  creado_en     timestamptz not null default now(),
  revocado_en   timestamptz null,
  revocado_por  uuid        null references perfil(id),
  ultimo_uso_en timestamptz null
);
create index pase_dispositivo_usuario_idx on pase_dispositivo (usuario_id)
  where revocado_en is null;

-- Antirreplay del pase: un código por dispositivo y paso temporal
create table pase_consumido (
  dispositivo_id uuid       not null references pase_dispositivo(id),
  paso           bigint     not null,
  consumido_en   timestamptz not null default now(),
  primary key (dispositivo_id, paso)
);

-- Notificaciones (RN-36..RN-39)
create table notificacion (
  id          uuid primary key default gen_random_uuid(),
  gimnasio_id uuid        not null references gimnasio(id),
  titulo      text        not null,
  cuerpo      text        not null,
  enviada_en  timestamptz not null default now(),
  enviada_por uuid        not null references perfil(id)
);

create table notificacion_lectura (
  notificacion_id uuid not null references notificacion(id),
  usuario_id      uuid not null references perfil(id),
  leida_en        timestamptz not null default now(),
  primary key (notificacion_id, usuario_id)
);

-- Nótese el sufijo: este dispositivo sólo recibe avisos. El que genera el
-- código de acceso es pase_dispositivo, más arriba.
create table dispositivo_push (
  id              uuid primary key default gen_random_uuid(),
  usuario_id      uuid        not null references perfil(id),
  expo_push_token text        not null unique,
  push_activo     boolean     not null default true,
  visto_en        timestamptz not null default now()
);
```

### Notas sobre el modelo

**`membresia` copia `plan_nombre` y `duracion_dias`.** Es desnormalización
deliberada: hace cumplir **RN-11** sin depender de versionar los planes.

**`sin_solape` es una restricción de exclusión, no un índice único.** Es lo que
impide de verdad la doble venta simultánea (**RN-13**); la validación en C# da el
mensaje de error legible, pero la garantía está en Postgres.

**`fecha_fin` es almacenada, no calculada al vuelo.** Los ajustes
(**RN-17**) la actualizan dentro de la misma transacción en la que se insertan,
para que el filtro de membresía vigente siga siendo un simple predicado sobre
fechas y pueda aprovechar índices.

**El estado de la membresía no se guarda** (**RN-16**). Guardar `vencida`
obligaría a un trabajo programado que lo actualice cada noche, y un trabajo
programado que no se ejecuta es un socio al que no se le deja entrar.

**El aforo no se materializa.** Es una consulta sobre `acceso` con el índice
parcial de arriba:

```sql
select count(distinct usuario_id)
from acceso
where gimnasio_id = @gym
  and resultado = 'permitido'
  and ocurrido_en > now() - make_interval(mins => @ventana);
```

A escala de un gimnasio esto son microsegundos. Materializarlo sería optimizar
antes de tener un problema.

---

## 6. Cálculo de la racha

Se resuelve en el servidor, nunca en el cliente (**RN-31** a **RN-35**).

1. Traer los días distintos con acceso permitido del usuario, convertidos a la
   zona horaria del gimnasio, de los últimos ~12 meses.
2. Agrupar por semana ISO y contar días por semana.
3. Recorrer semanas hacia atrás desde la última cerrada, sumando mientras cada
   semana alcance `dias_minimos_racha`.
4. Añadir la semana en curso si ya cumple; no romper la racha si todavía no.

El punto 1 es la única consulta a base de datos; el resto es lógica pura en
`Spotta.Domain`, que es exactamente lo que hace fácil probar los casos límite
(cambio de año, semana 53, horario de verano, usuario sin accesos).

---

## 7. Notificaciones push

El panel compone y envía; la API persiste primero en `notificacion` y después
entrega a la Expo Push API en lotes de 100 tokens.

Un fallo de entrega **no** es un fallo de la operación (**RN-37**): la
notificación ya existe en la bandeja de la aplicación. Los tokens que Expo marca
como `DeviceNotRegistered` se desactivan.

El envío ocurre fuera del ciclo de la petición HTTP, con un `BackgroundService` y
una cola en memoria. Si el proceso se reinicia a mitad, la notificación está
guardada y se puede reintentar; no se pierde el mensaje, como mucho el push.

---

## 8. Convenciones de la API

- Prefijo `/api`, versión en la ruta cuando haga falta romper algo.
- `/api/me/*` para todo lo que el socio consulta de sí mismo; el id sale del
  token.
- Errores con `ProblemDetails` (RFC 7807) y un `code` estable de negocio
  (`membresia_vigente_existente`, `dni_duplicado`, `codigo_incorrecto`), que es lo que
  el cliente traduce a texto. Las interfaces nunca muestran el mensaje crudo de
  la API.
- Paginación por cursor en los listados de usuarios y accesos.
- Zona horaria: la API recibe y devuelve instantes en UTC; la conversión a la
  zona del gimnasio es asunto suyo, no del cliente.

---

## 9. Decisiones registradas

| # | Decisión | Alternativa descartada | Motivo |
|---|---|---|---|
| D1 | Pase TOTP calculado en el dispositivo | QR estático con el id del usuario | Una captura de pantalla reenviada rompería el control de acceso |
| D1b | Generación sin red; validación con red | Pedir el código al servidor en cada uso | Los gimnasios están en sótanos; el socio no tiene cobertura y recepción sí |
| D2 | Supabase sólo como identidad y Postgres | Supabase como backend con RLS y PostgREST | La lógica de membresías y racha necesita un dominio probado, no políticas SQL |
| D3 | Cobro como columnas de `membresia` | Tabla `pago` separada | Una membresía es un cobro; una tabla aparte sólo se justifica con pagos parciales |
| D4 | Estado de membresía derivado | Campo `estado` con trabajo nocturno | Un trabajo programado que falla deja socios sin acceso |
| D5 | `gimnasio_id` desde la primera migración | Añadirlo cuando haga falta | Coste cero hoy, migración completa mañana (**RN-42**) |
| D6 | Aforo calculado por consulta | Contador materializado | Volumen de un gimnasio; evita desincronización |
| D7 | Contraseña inicial = DNI | Invitación por email con contraseña temporal | Decisión del producto; riesgo documentado en **RN-04** |
| D8 | Tres proyectos en la API | Un único proyecto | Reglas de negocio probables sin EF ni HTTP |
| D9 | Secreto compartido (TOTP) | Par de claves P-256 en el dispositivo | El par de claves evita guardar secretos en la base de datos, pero quintuplica el QR y añade una librería de curvas en React Native. Queda anotado como mejora: el riesgo que elimina sólo importa si alguien ya tiene la base de datos |

---

## 10. Entornos y configuración

El proyecto de Supabase es **`Spotta`**, referencia `mppkgpvejxbvlvatktqq`, en
**`sa-east-1` (São Paulo)**. Supabase no tiene región en Perú, y São Paulo es la
única sudamericana: unos 60-80 ms desde Lima, frente a los más de 180 ms de
cualquier región estadounidense. La región no se puede cambiar sin recrear el
proyecto.

Nombres reales de configuración de la API (`appsettings.json`, user-secrets o
entorno; en .NET los dos puntos se escriben `__` en una variable de entorno):

| Clave | Dónde | Para qué |
|---|---|---|
| `Supabase:Url` | API (`appsettings.json`) | endpoint del proyecto; no es secreto |
| `Supabase:ServiceRoleKey` | **sólo API, user-secrets** | alta de usuarios por la Admin API |
| `Supabase:JwtIssuer` | API | validación del JWKS |
| `ConnectionStrings:Postgres` | API, user-secrets | conexión Npgsql |
| `Pase:EncryptionKey` | API, user-secrets | cifrado de los secretos de pase |
| `Expo:AccessToken` | API, user-secrets | envío de push |
| `Cors:Origenes` | API | orígenes del panel web; el móvil no usa CORS |

En los clientes, los nombres los imponen sus herramientas:

| Variable | Dónde |
|---|---|
| `VITE_API_URL` | `Spotta.web` |
| `EXPO_PUBLIC_API_URL` | `Spotta.mobile` |
| `SUPABASE_URL`, `SUPABASE_ANON_KEY` | ambos, cuando entre el login en la Fase 1 |

Tres entornos: local (Supabase local o proyecto de desarrollo), `staging` y
producción. Las migraciones se aplican con EF Core desde el pipeline, nunca a
mano sobre producción.
