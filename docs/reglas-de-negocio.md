# Spotta — Reglas de negocio

Documento de referencia funcional. Cada regla tiene un identificador estable
(`RN-xx`) para poder citarla desde el código, los tests y las tareas.

Estado: acordado el 2026-11-04. Un cambio aquí es un cambio de producto, no un
detalle de implementación.

Los identificadores **no se reutilizan ni se renumeran**. Una regla nueva recibe
el siguiente número libre aunque viva en una sección anterior, igual que un
número de incidencia.

---

## 1. Identidad y usuarios

**RN-01 — No existe registro público.** La aplicación móvil no tiene pantalla de
alta. El único que crea usuarios es el personal de recepción con rol
`administrador`. Todos los endpoints de creación y edición de usuarios viven bajo
ese rol.

**RN-02 — Datos del usuario.** Nombre, apellidos, DNI, email y contraseña. El
email es el identificador de acceso.

**RN-03 — El documento es único.** No puede haber dos usuarios con el mismo
documento. Se admiten dos formatos peruanos:

- **DNI**: exactamente 8 dígitos.
- **Carné de extranjería (CE)**: 9 dígitos.

Se guarda normalizado, sin espacios ni guiones. **No se valida dígito
verificador**: el del DNI peruano no tiene un algoritmo público y estable, y
rechazar documentos válidos en el mostrador es peor que aceptar alguno
inventado, que de todas formas se detecta el día que el socio enseña el carné.

**RN-04 — Contraseña inicial.** Al crear el usuario, su contraseña es su propio
DNI, en mayúsculas y sin espacios ni guiones. El usuario puede cambiarla cuando
quiera desde su perfil; no se le obliga.

> **Riesgo asumido.** El DNI es un dato conocido por el personal y visible en la
> ficha del socio, así que la contraseña inicial es efectivamente pública dentro
> del gimnasio. Se mitiga con **RN-05** y **RN-06**, no se elimina.

**RN-05 — Aviso de contraseña por defecto.** Mientras el usuario no haya cambiado
su contraseña, el perfil muestra un aviso persistente invitándole a hacerlo. El
aviso desaparece tras el primer cambio.

**RN-06 — DNI enmascarado en listados.** El listado de usuarios del panel muestra
sólo los cuatro últimos caracteres del DNI. El DNI completo aparece al abrir la
ficha del usuario.

**RN-07 — Roles.** Dos roles: `administrador` (recepción) y `cliente` (socio). El
rol se asigna al crear la cuenta y viaja dentro del token de sesión.

**RN-08 — Ningún usuario se borra.** No existe borrado físico de usuarios. El
historial de accesos y de cobros debe permanecer íntegro.

**RN-09 — Baja lógica condicionada.** Un administrador puede dar de baja a un
usuario (marca de baja con fecha y autor) **sólo si no tiene una membresía
vigente**. Un usuario dado de baja no puede iniciar sesión y su QR es rechazado.
La baja es reversible: un administrador puede reactivarlo.

---

## 2. Planes de membresía

**RN-10 — Composición del plan.** Un plan tiene nombre, duración en días y
precio. Es el catálogo que gestiona recepción.

**RN-11 — El plan es editable; lo ya vendido, no.** Cambiar el precio o la
duración de un plan afecta únicamente a las membresías que se asignen a partir de
ese momento. Las membresías ya existentes conservan la duración y el precio con
los que se vendieron.

**RN-12 — Los planes no se borran, se archivan.** Un plan archivado no aparece al
asignar membresías nuevas, pero sigue existiendo para las membresías históricas
que lo referencian.

---

## 3. Membresías

**RN-13 — Una membresía vigente por usuario.** Un usuario no puede tener dos
membresías cuyos periodos se solapen. Se valida en el servicio y se impone además
en la base de datos con una restricción de exclusión por rango, para que dos
recepcionistas trabajando a la vez no puedan crear un duplicado.

**RN-14 — Nueva membresía sólo al finalizar la actual.** El usuario únicamente
puede adjudicarse una nueva membresía cuando la actual haya terminado. No hay
renovación anticipada ni encadenado automático.

**RN-15 — Cálculo de la fecha de fin.**
`fecha_fin = fecha_inicio + duracion_dias_del_plan + suma de ajustes`
La duración se copia del plan en el momento de la venta (ver **RN-11**).

**RN-16 — Estado de la membresía.** Derivado de las fechas, no almacenado como
verdad: `vigente` si hoy está dentro de `[fecha_inicio, fecha_fin]`, `finalizada`
si ya pasó, `programada` si aún no empieza.

**RN-17 — Ajuste de días.** Un administrador puede añadir días a una membresía
(compensación por cierre del gimnasio, lesión, cortesía comercial, corrección de
un error). Cada ajuste se registra como un movimiento con días, motivo
obligatorio, autor y fecha. Los ajustes pueden ser negativos para corregir, pero
**nunca pueden dejar la fecha de fin por debajo de la fecha de inicio**.

**RN-18 — No hay días de gracia automáticos.** Una membresía vencida es una
membresía vencida y el acceso se rechaza. Si procede dar margen a un socio, el
administrador le añade días explícitamente (**RN-17**), de forma que quede
registrado quién lo autorizó.

---

## 4. Cobros

**RN-19 — Registro de cobro manual.** Spotta no procesa pagos. Al asignar una
membresía se registra el importe cobrado, el método (efectivo, tarjeta,
transferencia), la fecha y **qué administrador lo registró**. El cobro se anota en
la propia membresía: una membresía equivale a un cobro.

**RN-20 — El importe cobrado puede diferir del precio del plan.** Se propone el
precio del plan como valor por defecto, pero es editable para admitir descuentos.
La diferencia queda visible en los informes.

---

## 5. Acceso con código QR

**RN-21 — El pase se calcula en el dispositivo.** La aplicación deriva un código
nuevo cada 30 segundos a partir de un secreto propio del dispositivo y de la hora
actual (TOTP), y lo muestra como QR. **No necesita conexión para generarlo**:
funciona en modo avión y en sótanos sin cobertura. Un QR capturado en pantalla
deja de valer en menos de un minuto.

> El gimnasio está en un sótano y el socio no tiene cobertura: ese es el caso
> normal, no la excepción. Pedir el código al servidor en la puerta dejaría el
> pase inservible justo donde hace falta. Quien sí tiene conexión siempre es
> recepción, y es ahí donde se valida.

**RN-22 — Validación en recepción.** Recepción escanea el QR y la API comprueba,
en este orden: formato del pase, que el dispositivo exista y no esté revocado,
que el código coincida con el calculado en el servidor, que ese código no se haya
consumido, que el usuario no esté de baja y que tenga membresía vigente.

**RN-23 — Todo veredicto se persiste.** Tanto los accesos permitidos como los
rechazados se guardan con su motivo. Motivos de rechazo: `pase_invalido`,
`codigo_incorrecto`, `pase_ya_usado`, `dispositivo_revocado`, `sin_membresia`,
`membresia_vencida`, `usuario_de_baja`.

**RN-24 — Qué ve la recepcionista.** El veredicto va acompañado del nombre y
apellidos del socio, el estado de su membresía y los días que le quedan. Un
semáforo sin contexto no sirve para atender a nadie.

**RN-25 — Reescaneo idempotente.** Si llega un código ya consumido, se mira si
ese usuario tiene un acceso permitido dentro de la ventana de presencia: si lo
tiene, la API responde «ya registrado» y no crea un segundo acceso; si no lo
tiene, se rechaza como `pase_ya_usado`. El reescaneo del socio que acaba de
entrar no infla el aforo ni altera la racha.

**RN-26 — Vía alternativa por DNI.** Recepción puede buscar a un socio por DNI o
por nombre y registrar su acceso manualmente. Cubre el móvil sin batería, la
aplicación sin instalar y el reloj desviado (**RN-45**). El acceso manual se
marca como tal y queda registrado con el administrador que lo autorizó.

**RN-44 — Enrolamiento y revocación del dispositivo.** El secreto del pase se
entrega **una sola vez**, al iniciar sesión con conexión, y se guarda en el
almacén seguro del sistema operativo. Cada dispositivo tiene el suyo y un socio
puede tener varios activos. Cerrar sesión revoca ese dispositivo, y un
administrador puede revocarlo desde el panel: a partir de ese momento sus
códigos dejan de valer, sin necesidad de que el dispositivo se entere.

**RN-45 — El reloj del dispositivo importa.** El código depende de la hora, así
que la aplicación guarda el desfase con el servidor cada vez que tiene conexión y
lo aplica al calcular. El servidor acepta un paso de tolerancia por arriba y por
abajo, lo que da 90 segundos de margen. Un desfase mayor se rechaza como
`codigo_incorrecto` y se resuelve por la vía manual (**RN-26**).

---

## 6. Aforo

**RN-27 — Cálculo del aforo.** El aforo actual es el número de **usuarios
distintos** con un acceso permitido en los últimos 60 minutos. Pasada esa hora se
asume que el usuario ya no está en el gimnasio.

**RN-28 — La ventana de presencia es de 60 minutos.** Valor fijo de producto. Se
almacena como configuración para poder ajustarlo sin desplegar, pero el valor
acordado es 60.

**RN-29 — Aforo máximo configurable.** El administrador define la capacidad
máxima del gimnasio. La ocupación se muestra como porcentaje sobre ese máximo.

**RN-30 — El aforo nunca bloquea el acceso.** Es información, no un control. Un
socio con membresía vigente entra siempre, aunque el gimnasio esté al 100 %. Si
la ocupación supera el máximo configurado, se muestra 100 % y se indica que está
por encima de la capacidad.

---

## 7. Racha

**RN-31 — La semana es de lunes a domingo.** Semana ISO, calculada en la zona
horaria del gimnasio. Calcularla en UTC desplazaría los entrenamientos del
domingo por la noche a la semana siguiente.

**RN-32 — Una semana cuenta si alcanza el mínimo de días.** El mínimo de días de
asistencia por semana lo configura el administrador. Valor por defecto: 3.

**RN-33 — Cómputo de la racha.** La racha es el número de semanas consecutivas
que cumplen **RN-32**, contando hacia atrás desde la última semana cerrada. La
semana en curso se suma si ya cumple el mínimo, pero **no rompe la racha mientras
no haya terminado**.

**RN-34 — Un día cuenta una vez.** Varios accesos el mismo día son un solo día de
asistencia.

**RN-35 — Vista de la semana.** La aplicación muestra qué días concretos de la
semana actual ha asistido el usuario (L M X J V S D) y el número de semanas de
racha acumuladas.

---

## 8. Notificaciones

**RN-36 — Sólo recepción envía.** Las notificaciones las compone y envía un
administrador desde el panel web, dirigidas a todos los usuarios con rol
`cliente`. No hay mensajería entre usuarios ni respuestas.

**RN-37 — Toda notificación se guarda.** El historial es persistente y vive en la
aplicación con independencia de que el envío push llegue o falle. El usuario la ve
al abrir la sección de notificaciones.

**RN-38 — El interruptor sólo afecta al push.** Si el usuario desactiva las
notificaciones, deja de recibir avisos en el dispositivo, pero los mensajes siguen
apareciendo en su bandeja dentro de la aplicación.

**RN-39 — Estado de lectura por usuario.** Se registra cuándo cada usuario leyó
cada notificación, para poder mostrar el contador de no leídas.

---

## 9. Perfil

**RN-40 — Contenido del perfil.** El usuario consulta sus datos personales
(nombre, apellidos, DNI, email), su membresía y días restantes, cambia su
contraseña, activa o desactiva las notificaciones y cierra sesión.

**RN-41 — El usuario no edita sus datos personales.** Nombre, apellidos, DNI y
email los modifica únicamente un administrador. El usuario sólo controla su
contraseña y sus preferencias de notificación.

---

## 10. Alcance

**RN-42 — Un solo gimnasio, preparado para varios.** Decidido el 2026-11-09:
Spotta es **monoinquilino**. La interfaz no tiene selector de sede y la API
resuelve el gimnasio desde su configuración. Todas las tablas de negocio llevan,
aun así, la referencia al gimnasio desde la primera migración, de modo que pasar
a multisede no exija migrar el esquema.

> **Lo que faltaría para que fuese multiinquilino**, por si algún día se decide:
> resolver el gimnasio desde el token en vez de la configuración; un
> `HasQueryFilter` por entidad en `SpottaDb`, para que ninguna consulta pueda
> olvidarse del filtro; el rol pasando de «administrador» a «administrador de la
> sede X»; y políticas de RLS por inquilino como segunda red.
>
> El filtro global es lo urgente de esa lista y lo único que hay que decidir
> pronto: meterlo antes de escribir las consultas cuesta horas, y después
> obliga a revisarlas una a una. El documento es único por gimnasio
> (**RN-03**), no global, así que esa parte ya está.

**RN-43 — Zona horaria y moneda** son configuración del gimnasio, no constantes en
el código. Valores iniciales: `America/Lima` y `PEN`.

> Perú no tiene horario de verano, así que el cálculo de racha (**RN-31**) no se
> enfrenta a semanas de 23 o 25 horas. El código sigue siendo consciente de la
> zona horaria de todas formas: la regla dice que la zona es configuración, y el
> día que haya una segunda sede en un país con cambio de hora, no se rehace.
