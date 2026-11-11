# Spotta — Roadmap

Ocho fases. Cada una termina en algo que se puede enseñar funcionando, no en «la
capa de datos está hecha». El camino crítico es **Fase 0 → 1 → 2 → 3**: hasta la
Fase 3 no existe producto.

Los módulos (`Mx`) están en [modulos.md](modulos.md); las reglas (`RN-xx`) en
[reglas-de-negocio.md](reglas-de-negocio.md).

---

## Fase 0 — Cimientos

**Objetivo** Que los tres clientes arranquen, se hablen entre ellos y haya una
base de datos con esquema versionado.

- Proyecto de Supabase creado; `btree_gist` habilitado.
- `Spotta.api`: solución .NET 10 con los cuatro proyectos en capas
  (`Api`, `Domain`, `Infrastructure`) y el de pruebas. EF Core + Npgsql.
- Primera migración con el esquema completo de
  [arquitectura.md](arquitectura.md), RLS activado y denegando.
- Semilla: el gimnasio, su configuración y un usuario administrador.
- `Spotta.web`: Vite + React + TypeScript, enrutado, cliente HTTP, variables de
  entorno.
- `Spotta.mobile`: Expo + TypeScript, cliente HTTP y la barra de cinco
  posiciones con deslizamiento entre pantallas, con los cinco glifos dibujados a
  medida y las pantallas aún vacías. La de `Entreno` ya queda en su estado de
  construcción definitivo.
- Endpoint `GET /api/salud` y una llamada real desde cada cliente.
- `README` de cada proyecto con cómo arrancarlo.

**Hecho cuando** `dotnet run`, `npm run dev` y `npx expo start` levantan los tres
y cada cliente pinta la respuesta de `/api/salud`.

**Decisiones a cerrar aquí** nombre del gimnasio y aforo inicial de la semilla.

---

## Fase 1 — Autenticación y usuarios

**Módulos** M1, M2

**Objetivo** El primer flujo completo de punta a punta: recepción crea un socio y
ese socio entra en la aplicación móvil.

- Validación de JWT de Supabase contra JWKS en la API; políticas
  `Administrador` y `Cliente`.
- Validador de documento peruano (DNI de 8 dígitos, CE de 9), en `Spotta.Domain`, con pruebas.
- Alta de usuario vía Admin API con contraseña igual al DNI y compensación si
  falla el perfil.
- Listado, ficha, edición, baja y reactivación.
- Login en web con guardia de rol; login y sesión persistente en móvil.

**Hecho cuando** se crea un socio en la web, se inicia sesión con él en el móvil
usando su DNI como contraseña, y ese mismo token es rechazado al llamar a un
endpoint de administración.

**Por qué va antes de todo** cualquier otra fase necesita un usuario autenticado
con rol. Además es donde se descubren los problemas de integración con Supabase,
y es mejor descubrirlos con poco código encima.

---

## Fase 2 — Planes y membresías

**Módulos** M3, M4

**Objetivo** Que exista el concepto de socio con derecho a entrar.

- CRUD de planes con archivado.
- Asignación de membresía con copia de duración y precio, registro del cobro y
  del administrador que lo registró.
- Restricción de exclusión por solape, probada con dos transacciones
  concurrentes.
- Ajustes de días con motivo obligatorio y auditoría.
- Vista de vencimientos próximos.
- Pantalla de membresía en el móvil con días restantes.

**Hecho cuando** cambiar el precio de un plan no altera ninguna membresía
vendida, la segunda membresía solapada falla, y «añadir 7 días» mueve la fecha de
fin y queda registrado quién y por qué.

---

## Fase 3 — QR y control de acceso

**Módulos** M5

**Objetivo** El producto, por fin. Un socio llega, muestra el móvil y recepción
sabe si entra.

- Enrolamiento del dispositivo: secreto entregado una vez y guardado en el
  almacén seguro del sistema.
- Generación local del código TOTP en el móvil, con corrección de reloj.
- Validación en el orden de **RN-22**, con tolerancia de ±1 paso, comparación en
  tiempo constante y tabla antirreplay.
- Revocación de dispositivos desde el panel.
- Escáner de cámara en la web, a pantalla completa, con veredicto grande.
- Pantalla de pase en el móvil con renovación automática del código.
- Acceso manual por DNI, marcado como manual.
- Registro de accesos del día en el panel.

**Hecho cuando** las siete situaciones pasan en un dispositivo real: vigente
entra, vencido no, dado de baja no, código caducado no, código repetido no, el
manual por DNI entra con su marca, y **el pase funciona con el móvil en modo
avión**.

**Riesgo** La cámara en el navegador necesita contexto seguro
(`localhost` o HTTPS). Preparar el certificado de desarrollo antes de empezar la
fase, no cuando el escáner no abra.

**Aquí es donde se enseña el proyecto.** A partir de este punto todo lo que
viene suma valor, pero nada es imprescindible para que el gimnasio funcione.

---

## Fase 4 — Aforo

**Módulos** M6

**Objetivo** Que el socio sepa si el gimnasio está lleno antes de salir de casa.

- Consulta de ocupación con el índice parcial.
- Aforo máximo editable en configuración.
- Bloque de aforo en la pantalla de Inicio, con tramos de lectura y sondeo cada
  60 s.
- Indicador en el panel de recepción.

**Hecho cuando** el contador refleja los accesos reales, no se duplica con el
reescaneo, y un acceso de hace 61 minutos ya no suma.

---

## Fase 5 — Racha

**Módulos** M7

**Objetivo** El motivo para abrir la aplicación un día que no se entrena.

- Cálculo de semanas ISO en la zona horaria del gimnasio, en `Spotta.Domain`.
- Mínimo de días por semana en configuración.
- Bloque de racha en la pantalla de Inicio: semanas acumuladas, días de la
  semana actual, histórico reciente.

**Hecho cuando** la batería de pruebas cubre los casos límite: sin accesos,
semana en curso incompleta, salto de año, horario de verano, varios accesos el
mismo día, y cambio del mínimo.

**Nota** Esta fase es casi toda lógica pura y presentación. Es la que mejor se
presta a construirla con pruebas por delante.

---

## Fase 6 — Notificaciones

**Módulos** M8

**Objetivo** Que recepción pueda avisar de un cierre, un cambio de horario o una
promoción.

- Registro de dispositivos Expo al iniciar sesión.
- Persistencia de la notificación y envío en lotes desde un `BackgroundService`.
- Desactivación automática de tokens caducados.
- Redacción, vista previa, confirmación e historial en el panel.
- Bandeja con no leídas e interruptor de push en el móvil.

**Hecho cuando** el mensaje llega al dispositivo, sigue apareciendo en la bandeja
de quien tiene el push desactivado, y el envío no bloquea la respuesta HTTP del
panel.

**Riesgo** El push requiere una build de desarrollo de Expo; no funciona en Expo
Go para notificaciones remotas en todas las plataformas. Contarlo con tiempo.

---

## Fase 7 — Perfil y acabado

**Módulos** M9 y el repaso transversal

**Objetivo** Que no parezca un prototipo.

- Perfil completo: datos, aviso de contraseña por defecto, cambio de contraseña,
  preferencias, cierre de sesión.
- Estados vacíos y de error en las dos aplicaciones, con los códigos de error de
  negocio traducidos a texto humano.
- Estados de carga y comportamiento sin conexión en el móvil.
- Accesibilidad: contraste, tamaños táctiles, lectores de pantalla en las
  pantallas principales.
- Builds de producción y despliegue de la API.

**Hecho cuando** un recorrido completo por las dos aplicaciones no muestra ni un
mensaje de error en inglés ni una pantalla en blanco.

---

## Orden y paralelismo

```
Fase 0 ─ Fase 1 ─ Fase 2 ─ Fase 3 ─┬─ Fase 4 ─┐
                                   ├─ Fase 5 ─┼─ Fase 7
                                   └─ Fase 6 ─┘
```

Las fases 4, 5 y 6 son independientes entre sí: tocan módulos distintos y no
comparten archivos más allá de la pantalla de configuración. Si en algún momento
quieres repartir trabajo, es ahí.

Las fases 0 a 3 son estrictamente secuenciales. Intentar adelantarlas en paralelo
produce código que hay que rehacer cuando el eslabón anterior cambia de forma.

---

## Qué queda deliberadamente fuera

Decisiones tomadas para no alargar el proyecto. Si alguna entra, entra como fase
propia, no como añadido a una existente.

- **Pasarela de pago.** El cobro se registra a mano (**RN-19**).
- **Multisede.** El esquema está preparado, la interfaz no (**RN-42**).
- **Reservas de clases y entrenadores.** Otro producto.
- **Informes y métricas de negocio.** Más allá de vencimientos y cobros del
  periodo.
- **Tiempo real.** El aforo se consulta por sondeo; no hay websockets.
- **Fichaje de salida.** La presencia se asume por la ventana de una hora
  (**RN-28**).
