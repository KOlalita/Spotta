# Spotta — Módulos

Nueve módulos. Cada uno declara qué resuelve, qué expone la API, qué aparece en
cada cliente, de qué depende y cuándo se considera terminado.

Las reglas citadas (`RN-xx`) están en [reglas-de-negocio.md](reglas-de-negocio.md).

---

## Mapa de dependencias

```
M1 Auth y roles
 ├── M2 Usuarios
 │    ├── M4 Membresías ──── M3 Planes
 │    │    └── M5 QR y acceso
 │    │         ├── M6 Aforo
 │    │         └── M7 Racha
 │    ├── M8 Notificaciones
 │    └── M9 Perfil
```

M5 es el centro del producto: M6 y M7 no son más que dos lecturas distintas de
los datos que M5 genera. Sin M5 no hay nada que mostrar.

---

## M1 — Autenticación y roles

**Resuelve** el acceso a ambos clientes y la separación entre administrador y
cliente (**RN-07**).

**API**
| Método | Ruta | Rol | Notas |
|---|---|---|---|
| — | — | — | El login lo hace el cliente contra Supabase Auth |
| `GET` | `/api/me` | Cliente | perfil del token, rol, estado de membresía |

La API no emite sesiones: valida el JWT de Supabase contra su JWKS y resuelve el
rol desde `app_metadata.role`.

**Web** Pantalla de login. Guardia de ruta que expulsa a quien no sea
administrador. Cierre de sesión.

**Móvil** Pantalla de login, sesión persistente, recuperación de contraseña por
email, cierre de sesión.

**Hecho cuando** un administrador entra en la web y es rechazado en una ruta de
cliente, un cliente entra en el móvil y la API rechaza su token en los endpoints
de administración, y la sesión sobrevive a cerrar y reabrir la aplicación.

---

## M2 — Usuarios

**Resuelve** el alta y la gestión de socios por parte de recepción
(**RN-01** a **RN-09**).

**API**
| Método | Ruta | Rol |
|---|---|---|
| `GET` | `/api/usuarios` | Administrador |
| `GET` | `/api/usuarios/{id}` | Administrador |
| `POST` | `/api/usuarios` | Administrador |
| `PUT` | `/api/usuarios/{id}` | Administrador |
| `POST` | `/api/usuarios/{id}/baja` | Administrador |
| `POST` | `/api/usuarios/{id}/reactivar` | Administrador |

El listado admite búsqueda por nombre, apellidos y DNI, filtro por estado de
membresía, y paginación por cursor. Devuelve el DNI enmascarado (**RN-06**).

El alta crea el usuario en Supabase con contraseña igual al DNI (**RN-04**) y
compensa borrándolo si falla la inserción del perfil.

**Web** Listado con búsqueda y filtros, formulario de alta con validación de DNI
en vivo, ficha del usuario con sus membresías y sus accesos, acciones de baja y
reactivación con confirmación.

**Móvil** Nada. El socio no se gestiona a sí mismo (**RN-41**).

**Hecho cuando** se puede crear un socio, verlo en el listado, iniciar sesión en
el móvil con su DNI como contraseña, y la baja se bloquea con un mensaje claro si
tiene membresía vigente.

---

## M3 — Planes de membresía

**Resuelve** el catálogo que recepción edita (**RN-10** a **RN-12**).

**API**
| Método | Ruta | Rol |
|---|---|---|
| `GET` | `/api/planes` | Administrador |
| `POST` | `/api/planes` | Administrador |
| `PUT` | `/api/planes/{id}` | Administrador |
| `POST` | `/api/planes/{id}/archivar` | Administrador |

**Web** Tabla de planes con edición en línea de nombre, duración y precio. Aviso
explícito, al editar, de que el cambio no afecta a las membresías ya vendidas.

**Móvil** El nombre del plan aparece en la membresía del socio; no hay catálogo.

**Hecho cuando** cambiar el precio de un plan no altera ninguna membresía
existente, y un plan archivado desaparece del selector de asignación pero sigue
visible en las membresías históricas.

---

## M4 — Membresías

**Resuelve** la venta, la vigencia y el ajuste de días (**RN-13** a **RN-20**).

**API**
| Método | Ruta | Rol |
|---|---|---|
| `GET` | `/api/usuarios/{id}/membresias` | Administrador |
| `POST` | `/api/usuarios/{id}/membresias` | Administrador |
| `POST` | `/api/membresias/{id}/ajustes` | Administrador |
| `GET` | `/api/membresias/vencimientos` | Administrador |
| `GET` | `/api/me/membresia` | Cliente |

`POST .../membresias` recibe plan, fecha de inicio, importe cobrado y método de
pago; copia nombre y duración del plan, calcula la fecha de fin y registra el
cobro con el administrador autenticado.

`POST .../ajustes` recibe días y motivo obligatorio, actualiza la fecha de fin en
la misma transacción y deja el movimiento auditado.

**Web** Asignación de membresía desde la ficha del socio, con el importe del plan
precargado y editable. Historial de membresías con sus cobros y ajustes. Botón
«Añadir días» con motivo obligatorio. Vista de vencimientos próximos.

**Móvil** Membresía actual, plan, fecha de fin y días restantes. Si está vencida,
mensaje de que debe pasar por recepción.

**Hecho cuando** intentar asignar una segunda membresía solapada falla con un
mensaje legible, el intento simultáneo desde dos pestañas falla también (lo corta
la restricción de exclusión), y añadir 7 días mueve la fecha de fin y aparece en
el historial con su motivo y su autor.

---

## M5 — QR y control de acceso

**Resuelve** el flujo central del producto (**RN-21** a **RN-26**).

**API**
| Método | Ruta | Rol |
|---|---|---|
| `POST` | `/api/me/pase` | Cliente |
| `DELETE` | `/api/me/pase` | Cliente |
| `POST` | `/api/check-ins` | Administrador |
| `POST` | `/api/check-ins/manual` | Administrador |
| `GET` | `/api/check-ins` | Administrador |
| `GET` | `/api/usuarios/{id}/accesos` | Administrador |
| `POST` | `/api/usuarios/{id}/dispositivos/{dId}/revocar` | Administrador |

**Web** Pantalla de escaneo con la cámara, a pantalla completa, pensada para
estar abierta todo el día. Veredicto grande e inequívoco con el nombre del socio
y los días que le quedan. Buscador por DNI para el acceso manual. Registro de los
accesos del día.

**Móvil** Enrolamiento del dispositivo al iniciar sesión y guardado del secreto
en el almacén seguro del sistema. Pantalla de pase: el QR como elemento
protagonista, generado **en local** cada 30 segundos, con el nombre del socio, su
plan y el indicador de renovación.

**Hecho cuando** un socio con membresía vigente entra, un socio vencido es
rechazado con el motivo correcto, **el pase se genera y se acepta con el móvil en
modo avión**, un QR de hace dos minutos es rechazado, el mismo QR escaneado dos
veces no crea un segundo acceso, un dispositivo revocado deja de valer al
instante, y el acceso manual queda marcado como manual con su autor.

**Riesgo** La cámara del navegador exige contexto seguro: en desarrollo hay que
servir la web por `localhost` o con certificado, porque `getUserMedia` no
funciona sobre HTTP en red local.

---

## M6 — Aforo

**Resuelve** mostrar cuánta gente hay ahora en el gimnasio (**RN-27** a
**RN-30**).

**API**
| Método | Ruta | Rol |
|---|---|---|
| `GET` | `/api/aforo` | Cliente |
| `GET` | `/api/configuracion` | Administrador |
| `PUT` | `/api/configuracion` | Administrador |

`/api/aforo` devuelve ocupación actual, máximo y porcentaje. Se consulta por
sondeo cada 60 segundos desde el móvil; no justifica tiempo real.

**Web** Campo de aforo máximo en configuración. Indicador de ocupación en el
panel principal, útil para recepción.

**Móvil** Bloque superior de la pantalla de **Inicio**: indicador de ocupación
con tres tramos de lectura (holgado, concurrido, lleno) y el número de personas.

**Hecho cuando** registrar un acceso incrementa la ocupación, el mismo usuario
escaneando dos veces no la incrementa dos veces, un acceso de hace 61 minutos ya
no cuenta, y cambiar el aforo máximo cambia el porcentaje sin tocar los accesos.

---

## M7 — Racha

**Resuelve** la vista de constancia del socio (**RN-31** a **RN-35**).

**API**
| Método | Ruta | Rol |
|---|---|---|
| `GET` | `/api/me/racha` | Cliente |

Devuelve semanas consecutivas, días de la semana en curso, mínimo configurado y
las últimas semanas con su recuento, para poder dibujar la evolución.

**Web** El mínimo de días por semana, en configuración.

**Móvil** Bloque inferior de la pantalla de **Inicio**: número de semanas, los
siete días de la semana actual marcados, y el histórico de semanas recientes.

**Hecho cuando** las pruebas cubren: usuario sin accesos, semana en curso
incompleta que no rompe la racha, cambio de año entre semana 52 y 1, varios
accesos el mismo día contando como uno, y cambio del mínimo recalculando el
resultado.

---

## M8 — Notificaciones

**Resuelve** el aviso masivo de recepción a los socios (**RN-36** a **RN-39**).

**API**
| Método | Ruta | Rol |
|---|---|---|
| `POST` | `/api/notificaciones` | Administrador |
| `GET` | `/api/notificaciones` | Administrador |
| `GET` | `/api/me/notificaciones` | Cliente |
| `POST` | `/api/me/notificaciones/{id}/leida` | Cliente |
| `POST` | `/api/me/dispositivos` | Cliente |
| `PUT` | `/api/me/dispositivos/push` | Cliente |

**Web** Redacción con título y cuerpo, vista previa, confirmación antes de enviar
(es irreversible y llega a todos los socios) e historial de lo enviado.

**Móvil** Registro del token de Expo al iniciar sesión, bandeja como pestaña
propia (`Avisos`, segunda posición de la barra) con un punto de lima sobre el
glifo cuando hay sin leer, detalle del mensaje, interruptor de push en el
perfil.

**Hecho cuando** una notificación enviada aparece en la bandeja de un socio que
tiene el push desactivado, los tokens caducados se desactivan solos, y el
historial del panel muestra cuántos envíos se entregaron.

---

## M9 — Perfil

**Resuelve** lo que el socio controla de su propia cuenta (**RN-40**,
**RN-41**).

**API**
| Método | Ruta | Rol |
|---|---|---|
| `GET` | `/api/me` | Cliente |
| `POST` | `/api/me/contrasena` | Cliente |

**Móvil** Datos personales en lectura, aviso de contraseña por defecto mientras
no la haya cambiado (**RN-05**), cambio de contraseña, interruptor de
notificaciones, cierre de sesión.

**Hecho cuando** el aviso de contraseña desaparece tras cambiarla, el cambio se
propaga a Supabase, y cerrar sesión limpia el token y el registro del
dispositivo.
