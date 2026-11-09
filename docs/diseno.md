# Spotta — Diseño

Dirección visual, sistema de tokens y recorrido pantalla a pantalla.

La dirección de color quedó fijada el 2026-11-03 y **no está en revisión**: tinta
casi negra con lima eléctrica como único acento, y la tarjeta de pase en oliva.
La paleta de teales anterior quedó descartada.

---

## 1. Dirección

Spotta es una aplicación que se usa de pie, con prisa, en un pasillo con mala
luz: diez segundos en la puerta del gimnasio y treinta mirando la racha. Eso
manda sobre todo lo demás.

Tres principios:

**El pase manda.** La pantalla del QR es la razón de existir de la aplicación
móvil. Es la pantalla de inicio, ocupa el ancho completo y no comparte espacio
con nada secundario.

**Un solo acento.** La lima aparece en elementos pequeños: pestaña activa,
insignias, punto de estado, foco. Las superficies grandes son tinta u oliva.
Un acento que lo pinta todo deja de señalar.

**Números grandes, texto poco.** Aforo, días restantes y semanas de racha son
cifras que se leen de un vistazo. Todo lo demás es contexto en letra pequeña.

---

## 2. Tokens de color

### Base oscura — aplicación móvil y pantalla de escaneo

| Token | Valor | Uso |
|---|---|---|
| `ink-900` | `#0B0E0C` | fondo de la aplicación |
| `ink-800` | `#141814` | superficie de tarjeta |
| `ink-700` | `#1E231E` | superficie elevada, campos |
| `ink-600` | `#2A302A` | bordes y separadores |
| `text-hi` | `#F4F6F2` | texto principal |
| `text-mid` | `#A3ABA2` | texto secundario |
| `text-lo` | `#6E776D` | texto deshabilitado |

### Marca

| Token | Valor | Uso |
|---|---|---|
| `lime` | `#D4FF3F` | acento único: pestaña activa, insignias, punto de estado, anillo de foco |
| `lime-press` | `#BFE838` | estado pulsado del acento |
| `olive` | `#6A8019` | superficie de la tarjeta de pase y del veredicto permitido |

**Restricción del oliva.** `#6A8019` con texto blanco da 4,45:1 de contraste: es
suficiente para texto grande (desde 24 px, o 19 px en negrita) pero no para texto
pequeño. En la tarjeta de pase, la letra menuda va en tinta sobre un recorte
claro, no en blanco sobre el oliva. Y el oliva no se aclara: es el tono más claro
que todavía sostiene texto blanco.

**Restricción de la lima.** `#D4FF3F` es un amarillo verdoso muy luminoso: sobre
tinta brilla, sobre blanco es ilegible. Nunca se usa como color de texto en
fondos claros. En el panel web aparece sólo como relleno detrás de texto en
tinta, o como indicador pequeño con borde.

### Semánticos

| Token | Valor | Uso |
|---|---|---|
| `ok` | `#6A8019` / `#D4FF3F` | permitido, holgado, activo |
| `warn` | `#F5A524` | concurrido, membresía por vencer |
| `danger` | `#E5484D` | rechazado, lleno, membresía vencida, baja |

No se introduce un verde propio para «correcto»: el verde de la marca es la
lima, y meter un segundo verde al lado la ensucia.

### Base clara — panel de recepción

**Propuesta a confirmar.** La aplicación móvil es oscura. Para el panel web
propongo base clara: la recepcionista pasa ocho horas leyendo tablas densas bajo
fluorescentes, y ahí el fondo claro cansa menos y las tablas respiran. Mismos
tokens de marca, invertida la escala.

| Token | Valor | Uso |
|---|---|---|
| `paper` | `#FBFCFA` | fondo |
| `paper-card` | `#FFFFFF` | superficie |
| `line` | `#E4E8E1` | bordes de tabla |
| `text-hi` | `#0B0E0C` | texto principal (la misma tinta) |
| `text-mid` | `#5C645B` | texto secundario |

**Excepción:** la pantalla de escaneo del panel es oscura, con la base `ink`.
Funciona como un expositor en el mostrador, no como una herramienta de lectura,
y el veredicto se tiene que ver desde metro y medio.

---

## 3. Tipografía

**Inter** en el panel web. En el móvil, la fuente del sistema (San Francisco y
Roboto): se cargan al instante y el socio está acostumbrado a ellas.

Cifras siempre con **numeración tabular** (`font-variant-numeric: tabular-nums`),
para que el contador de aforo no baile al cambiar de 7 a 8.

| Rol | Tamaño / peso | Dónde |
|---|---|---|
| `display` | 56 / 700 | aforo, semanas de racha |
| `title` | 28 / 600 | cabeceras de pantalla |
| `subtitle` | 19 / 600 | nombre en el pase, veredicto |
| `body` | 16 / 400 | texto general |
| `caption` | 13 / 500 | etiquetas, metadatos |
| `mono` | 14 / 500 | DNI, identificadores |

El DNI en monoespaciada: es una cadena que se compara carácter a carácter.

---

## 4. Forma y espacio

Escala de espaciado en múltiplos de 4: `4 · 8 · 12 · 16 · 24 · 32 · 48`.

| Radio | Valor | Uso |
|---|---|---|
| `r-sm` | 8 | campos, insignias |
| `r-md` | 14 | tarjetas, botones |
| `r-lg` | 24 | tarjeta de pase |
| `r-full` | 999 | píldoras, puntos de estado |

Sombras: ninguna en la base oscura. La elevación se expresa con `ink-700` sobre
`ink-800`, no con sombras que sobre tinta no se ven. En el panel claro, una sola
sombra suave para las tarjetas.

Objetivos táctiles de 44 px como mínimo, siempre.

---

## 5. Aplicación móvil

**Sólo teléfono, sólo vertical.** No hay disposición para tableta ni para
horizontal; la orientación se bloquea en `portrait`. Todas las medidas de este
documento asumen un ancho de 360 a 430 px.

### Navegación

Barra inferior flotante de **cinco posiciones**, con el pase en el centro,
elevado por encima de la barra:

```
   ╭───────────────────────────────────────────╮
   │                   ╭───╮                   │
   │    ⌂       ♤      │ ▦ │      ⊞      ◯     │
   │  Inicio  Avisos   ╰───╯   Entreno  Perfil │
   ╰───────────────────────────────────────────╯
```

| Posición | Pantalla | Etiqueta | Glifo |
|---|---|---|---|
| 1 | Inicio: aforo y racha | `Inicio` | casa |
| 2 | Notificaciones | `Avisos` | campana |
| 3 | **Pase** | sin etiqueta | código QR |
| 4 | Entreno (en construcción) | `Entreno` | mancuerna |
| 5 | Perfil | `Perfil` | persona |

**Inicio reúne aforo y racha** en una sola pantalla. Son las dos cifras que el
socio consulta de pasada, ninguna da para una pantalla entera, y juntas forman
el resumen que una casa promete.

**La aplicación abre en Inicio.** Si la primera posición es una casa, tiene que
comportarse como una casa; una casa que no es el punto de partida confunde. El
pase sigue a un toque de distancia desde cualquier pantalla, y es el objetivo
táctil más grande de toda la interfaz.

> Si prefieres que la aplicación abra directamente en el pase, entonces la
> primera posición no debería ser una casa: sería «Resumen» con otro glifo. Las
> dos opciones son defendibles, pero el icono y el comportamiento tienen que
> decir lo mismo.

**La campana de la cabecera desaparece.** Las notificaciones tienen pestaña
propia y no hay motivo para duplicar el acceso. El indicador de no leídas es un
punto de lima sobre el glifo de `Avisos`.

**«Avisos», no «Notificaciones».** En una barra de cinco, la etiqueta larga se
trunca o te obliga a bajar el cuerpo por debajo de 11 px.

**Entreno queda reservado.** Cuarta posición ocupada desde el primer día, con su
pantalla en construcción (ver más abajo). Reservar el hueco ahora evita que
añadirlo más tarde reordene una barra que el socio ya tiene memorizada.

### Barra: medidas

| Elemento | Valor |
|---|---|
| Alto de la barra | 64 |
| Margen lateral | 16 |
| Margen inferior | `safe-area-bottom` + 12 |
| Radio | 28 |
| Fondo | `ink-800` |
| Icono lateral | 24, trazo 1,75 |
| Etiqueta | 11 / 500, a 4 px del icono |
| Activo | glifo y etiqueta en `lime` |
| Inactivo | glifo y etiqueta en `text-mid` |
| Reservado (`Entreno`) | glifo y etiqueta en `text-lo`, siempre |

Sin sombra: sobre `ink-900` no se vería. La barra se separa del fondo por su
propio tono, que es un escalón más claro.

### Glifos: dibujados, no importados

**Regla.** Los cinco glifos de la barra se dibujan a medida para Spotta, como
SVG propios del proyecto. **Prohibido usar fuentes de iconos o paquetes
genéricos** —  Tabler, Font Awesome, Ionicons, Material Icons, Feather, Lucide —
en la barra de navegación.

**Por qué.** Son los cinco símbolos que el socio ve en cada pantalla, cada vez
que abre la aplicación. Un paquete genérico los convierte en los mismos cinco
iconos que tienen otras cien aplicaciones: la barra deja de tener autor y se nota
al instante. Es el detalle que separa un producto de una plantilla.

**Cómo se dibujan.**

| Parámetro | Valor |
|---|---|
| Lienzo | 24 × 24 |
| Trazo | 1,75, extremos y uniones redondeados |
| Relleno | ninguno, salvo los módulos del QR |
| Rejilla | geometría sobre media unidad; nada a 0,3 px |
| Óptica | mismo peso visual entre los cinco: la casa y la mancuerna tienden a pesar más que la campana, así que se ajustan a ojo, no por caja |

Los cinco: casa (tejado y cuerpo, sin puerta ni ventanas), campana (cuerpo y
badajo), código QR (tres marcas de posición y tres módulos), mancuerna (barra y
dos discos por lado), persona (cabeza y hombros).

Fuera de la barra, en listas y formularios, sí se puede usar un paquete de
iconos: ahí el símbolo es funcional y nadie lo mira dos veces. La regla protege
la barra, que es la firma de la aplicación.

### Botón central

Cuatro capas concéntricas, de fuera hacia dentro:

| Capa | Valor | Para qué |
|---|---|---|
| Anillo activo | 2, `lime` | sólo cuando el pase es la pantalla actual |
| Anillo de separación | 6, `ink-900` | recorta el círculo contra la barra |
| Círculo | Ø 64, `olive` | la superficie del botón |
| Glifo | 28, blanco | el código QR |

El relleno en **oliva** y no en lima es deliberado: es el mismo tono de la
tarjeta de pase que abre, así que el botón se lee como la puerta de entrada a esa
tarjeta. La lima se reserva para el anillo de estado, que es un trazo de 2 px.

El círculo sobresale **22 px** por encima del borde superior de la barra. Al
pulsarlo o al llegar deslizando, escala a 1,06 en 120 ms y vuelve.

### Deslizar entre pantallas

Las cinco pantallas son un carrusel horizontal: se cambia de pantalla tocando la
pestaña **o deslizando el dedo**, y las dos vías mueven el mismo indicador.

- El gesto es continuo: al arrastrar a medias, el color de los iconos interpola
  entre `text-mid` y `lime` según la posición real del carrusel. Soltar a menos
  de un tercio devuelve a la pantalla de origen.
- No hay salto circular: desde `Aforo` no se llega a `Perfil` deslizando a la
  izquierda. Los extremos topan con un rebote corto.
- El botón central también sigue el gesto: su anillo de lima aparece de forma
  progresiva al acercarse la pantalla del pase.
- Tocar la pestaña ya activa lleva el contenido al principio.

**Ningún contenido desliza en horizontal.** Es la regla que hace que el gesto
funcione: cualquier carrusel, tabla desplazable o galería dentro de una pantalla
competiría con el cambio de pestaña y produciría el peor de los fallos de
interfaz, el que depende de dónde pusiste el dedo. Si alguna pantalla necesita
mostrar más de lo que cabe a lo ancho, se rediseña para que quepa.

### Cómo se construye

Un carrusel con barra propia, no una barra de pestañas estándar: las de
`@react-navigation/bottom-tabs` no deslizan. La vía corta es
`@react-navigation/material-top-tabs` (que ya usa `react-native-pager-view`) con
`tabBarPosition: "bottom"` y un `tabBar` propio.

Dos detalles que cuestan una tarde si se descubren tarde:

**En Android, lo que sobresale del contenedor no recibe toques.** El círculo no
puede desbordar la barra: el contenedor de la barra tiene que extenderse 28 px
hacia arriba con fondo transparente, y el círculo vivir dentro de esos límites.

**El `safe-area-bottom` no es opcional.** En los teléfonos con indicador de
inicio, una barra pegada al borde queda debajo del gesto del sistema. Se calcula
con `react-native-safe-area-context`, no con una constante.

### Pase

La pantalla protagonista, construida como un billete.

- Fondo `ink-900`.
- Tarjeta de pase en `olive`, radio 24, ocupando el ancho menos 20 px por lado.
- Dentro de la tarjeta, un recorte claro centrado con el QR: el código necesita
  fondo claro para que cualquier lector lo lea.
- Sobre el QR: nombre y apellidos en `subtitle` blanco; debajo, el plan.
- Bajo el recorte, el muesca de billete (dos semicírculos en los bordes y una
  línea discontinua) y la letra menuda en tinta: días restantes y fecha de fin.
- Indicador de renovación: un anillo fino de lima que se vacía en 30 segundos.
  Al llegar a cero, el QR se sustituye solo, sin parpadeo ni botón.
- **Sin conexión el pase funciona igual.** El código se calcula en el
  dispositivo, así que no hay estado degradado, ni aviso, ni mensaje de error:
  la pantalla es idéntica en modo avión. Es la razón de ser del mecanismo.
- La única excepción es el dispositivo aún sin enrolar — recién instalado y sin
  haber iniciado sesión con conexión. Entonces, en lugar del QR, una línea:
  «Conéctate una vez para activar tu pase».

### Inicio

Una sola pantalla con dos bloques, en este orden: primero el aforo, que es
información del momento y caduca; después la racha, que es historia y espera.

Cabecera con el saludo y el nombre de pila, y debajo los dos bloques como
tarjetas en `ink-800`. Deslizar hacia abajo refresca ambos.

**Bloque de aforo**

- Cifra `display` con el número de personas.
- Debajo, barra de progreso de una sola línea sobre el aforo máximo.
- Tramos: hasta 50 % **holgado** (punto lima), hasta 85 % **concurrido**
  (ámbar), por encima **lleno** (rojo).
- Pie en `caption`: «actualizado hace un momento». Se refresca cada 60 s y al
  volver a primer plano.
- Por encima del máximo: la barra se queda al 100 % y el texto dice «por encima
  de la capacidad». No se muestran porcentajes superiores a 100.

**Bloque de racha**

- Cifra `display` con las semanas, y debajo «semanas seguidas».
- Fila de siete casillas `L M X J V S D` para la semana en curso: asistido en
  lima, hoy con anillo, resto en `ink-700`.
- Debajo, histórico de las últimas doce semanas como columnas de altura
  proporcional a los días asistidos, con las que alcanzaron el mínimo en lima y
  las que no en `ink-600`. Las doce columnas **caben en el ancho de la
  pantalla**: a 360 px son columnas de 16 con 8 de separación. No se desplaza en
  horizontal, porque competiría con el gesto de cambiar de pantalla.
- Texto de apoyo con el mínimo vigente: «cuentan las semanas con 3 días o más».
- Sin accesos todavía: una sola frase, sin gráfico vacío, invitando a escanear
  en la puerta.

### Avisos

Lista en orden inverso. No leídas con un punto de lima a la izquierda y título en
`text-hi`; leídas con título en `text-mid`. Se marcan al abrirlas. Vacío: una
línea de texto, nada más.

### Entreno — en construcción

Pantalla reservada para rutinas y seguimiento de entrenamiento. Hasta que exista,
muestra un estado de espera, no una pantalla rota.

- Glifo de mancuerna grande, centrado, en `ink-600`: apagado, no en lima. Nada
  que parezca pulsable.
- Título: «Entreno».
- Una línea en `text-mid`: «Rutinas y seguimiento. Estamos construyéndolo».
- Sin botones, sin barra de progreso falsa, sin fecha prometida. Una fecha que
  se incumple es peor que no dar ninguna.
- La pestaña se mantiene en `text-lo` incluso cuando esta es la pantalla actual:
  es la única del conjunto que nunca se ilumina en lima, porque no hay nada
  activo que señalar.

Deslizar hasta aquí funciona igual que al resto: la pantalla es real, sólo está
vacía a propósito.

### Perfil

- Datos personales en lectura, en una tarjeta. El DNI en monoespaciada.
- Si la contraseña sigue siendo el DNI, aviso ámbar persistente arriba con un
  botón para cambiarla.
- Cambio de contraseña, interruptor de notificaciones, cierre de sesión al final
  y en `danger`.

---

## 6. Panel de recepción

### Estructura

Barra lateral fija: **Inicio · Escanear · Socios · Planes · Accesos ·
Notificaciones · Configuración**.

«Escanear» es la segunda entrada a propósito: es la acción que más veces se hace
al día y debe estar a un clic desde cualquier sitio.

### Inicio

Tres cifras arriba (ocupación actual, socios con membresía vigente, cobrado este
mes) y, debajo, la lista de vencimientos de los próximos siete días, que es la
tarea comercial real de recepción.

### Escanear

Pantalla completa, base oscura, sin barra lateral.

- Vista de la cámara centrada, con un marco de lima.
- El veredicto **cubre la pantalla**, no es un aviso en una esquina:
  - **Permitido**: fondo `olive`, marca de verificación, nombre y apellidos en
    grande, y los días restantes. Si quedan 7 días o menos, el dato en ámbar.
  - **Rechazado**: fondo `danger`, cruz, nombre si se pudo identificar, y el
    motivo en lenguaje llano («Membresía vencida el 12 de octubre»), nunca el
    código interno.
- El veredicto se mantiene 4 segundos y vuelve a la cámara, o se cierra con
  cualquier tecla.
- Buscador por DNI o nombre siempre accesible en la parte inferior, para el
  acceso manual.
- Un sonido corto distinto para permitido y rechazado: en un mostrador ruidoso,
  la recepcionista escucha antes de mirar.

### Socios

Tabla densa: nombre, DNI enmascarado, estado de membresía como píldora, fecha de
fin, último acceso. Búsqueda arriba, filtro por estado. La fila entera abre la
ficha.

Ficha del socio en tres bloques: datos personales, membresías (con sus cobros y
ajustes) y accesos recientes. Las acciones destructivas (baja) al final, no junto
a las de uso diario.

### Planes

Tabla corta, edición en línea. Al cambiar precio o duración, un aviso
inmediatamente debajo: «afecta sólo a las membresías que asignes a partir de
ahora».

### Notificaciones

Redacción a la izquierda, vista previa a la derecha con la apariencia real de la
notificación en el móvil. Botón de envío con confirmación que dice a cuántos
socios va a llegar; es irreversible.

---

## 7. Reglas transversales

**Ningún código de error a la vista.** Cada código de negocio de la API tiene su
traducción en la interfaz. `membresia_vigente_existente` se lee como «Este socio
ya tiene una membresía activa hasta el 30 de noviembre».

**Estados vacíos con una frase.** Sin ilustraciones ni gráficos a cero. Una línea
que diga qué va a aparecer ahí y, si procede, qué hacer.

**Carga sin saltos.** Esqueletos del tamaño final del contenido, para que la
pantalla no se reorganice al llegar los datos.

**Fechas en lenguaje natural** en el móvil («quedan 12 días», «vence el 30 de
noviembre») y en formato corto en las tablas del panel, donde se comparan entre
sí.

**Mayúsculas.** Ni una palabra en mayúsculas sostenidas, salvo las iniciales de
los días de la semana en la racha.
