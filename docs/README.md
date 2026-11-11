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

**Fases 0 y 1 terminadas.**

La API de usuarios está completa y probada de punta a punta contra el Supabase
real, con tokens de verdad: 39 comprobaciones, entre ellas las del criterio de
terminado — un socio creado por recepción inicia sesión con su DNI, y su token
es rechazado en los endpoints de administración; el de un administrador, en
`/api/me`.

- **API**: autenticación por JWKS, roles, alta, listado con búsqueda, filtro y
  cursor, ficha, edición, baja con bloqueo de la cuenta y reactivación.
- **Panel web**: login con guardia de rol, listado, alta con validación del DNI
  en vivo, ficha en tres bloques, edición, baja y reactivación con confirmación.
- **Móvil**: login, sesión que sobrevive a cerrar la aplicación, rechazo de las
  cuentas de recepción, perfil, cambio de contraseña y cierre de sesión.
- **Contraseñas sin correo** (decisión D13): el socio cambia la suya desde el
  perfil; si la olvida, recepción la restablece al DNI desde la ficha.

Queda un socio de prueba: `socio.prueba@spotta.pe`, documento `70000001`.

El gimnasio es peruano: `America/Lima`, soles y documento de 8 dígitos (DNI) o 9
(carné de extranjería). Perú no tiene horario de verano, lo que le quita un caso
límite al cálculo de racha.

Pendiente de confirmar: la base clara del panel de recepción, propuesta en
[diseno.md](diseno.md#base-clara--panel-de-recepción).
