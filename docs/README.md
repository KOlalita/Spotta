# Spotta — Documentación

Control de acceso y gestión de socios para un gimnasio. Los socios se identifican
en recepción con un código QR; si su membresía está vigente, entran.

| Documento | Qué contiene |
|---|---|
| [reglas-de-negocio.md](reglas-de-negocio.md) | Las 43 reglas funcionales acordadas, con identificador estable `RN-xx` |
| [arquitectura.md](arquitectura.md) | Stack, identidad, token de QR, modelo de datos SQL, decisiones registradas |
| [modulos.md](modulos.md) | Los nueve módulos: endpoints, pantallas, dependencias y criterio de terminado |
| [roadmap.md](roadmap.md) | Las ocho fases de desarrollo y lo que queda fuera del alcance |
| [diseno.md](diseno.md) | Dirección visual, tokens de color y tipografía, y recorrido pantalla a pantalla |

## Las piezas

| Proyecto | Stack | Para quién |
|---|---|---|
| `Spotta.api` | C# / .NET 10, EF Core, Npgsql | — |
| `Spotta.web` | React + Vite + TypeScript | recepción (rol administrador) |
| `Spotta.mobile` | React Native + Expo | socios (rol cliente) |

Base de datos PostgreSQL en Supabase. Supabase Auth como proveedor de identidad.
Todo el tráfico pasa por la API: ningún cliente habla directamente con la base de
datos.

## Cómo leer esto

Si vas a implementar algo, el orden es: la fase en
[roadmap.md](roadmap.md) → el módulo en [modulos.md](modulos.md) → las reglas que
cita → el modelo de datos en [arquitectura.md](arquitectura.md).

Las reglas de negocio son la referencia que manda. Si el código y una regla no
coinciden, lo que está mal es el código, o hay que cambiar la regla
explícitamente.

## Estado

**Fase 0 terminada**, salvo un paso que sólo puede dar Adriano.

Hecho: los tres proyectos arrancan y se hablan; el proyecto de Supabase
`Spotta` está creado en São Paulo con las dos migraciones aplicadas (11 tablas,
RLS activado, la restricción de exclusión y la llave contra `auth.users`); y el
gimnasio está sembrado — `Spotta Gym`, `America/Lima`, `PEN`, aforo 80, ventana
de 60 minutos, 3 días mínimos de racha.

Pendiente:

1. Restablecer la contraseña de la base de datos en el panel de Supabase y
   guardarla en user-secrets. Hasta entonces `/api/salud` responde
   `sin conexion`.
2. Guardar también `Supabase:ServiceRoleKey` y crear el primer administrador con
   `sembrar-admin` (ver el [README de la API](../Spotta.api/README.md)).

El gimnasio es peruano: `America/Lima`, soles y documento de 8 dígitos (DNI) o 9
(carné de extranjería). Perú no tiene horario de verano, lo que le quita un caso
límite al cálculo de racha.

Pendiente de confirmar: la base clara del panel de recepción, propuesta en
[diseno.md](diseno.md#base-clara--panel-de-recepción).
