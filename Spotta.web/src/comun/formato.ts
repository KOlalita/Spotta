/*
 * Formato corto en las tablas del panel, donde las fechas se comparan entre sí
 * (diseño, sección 7).
 */

const SIN_HORA = new Intl.DateTimeFormat("es-PE", {
  day: "2-digit",
  month: "short",
  year: "numeric",
  timeZone: "UTC",
});

const CON_HORA = new Intl.DateTimeFormat("es-PE", {
  day: "2-digit",
  month: "short",
  hour: "2-digit",
  minute: "2-digit",
});

const SOLES = new Intl.NumberFormat("es-PE", { style: "currency", currency: "PEN" });

/**
 * Una fecha sin hora, como "2026-12-09". Se formatea en UTC a propósito: con
 * la zona del navegador, new Date("2026-12-09") es la medianoche UTC, que en
 * Lima todavía es el día 8, y la fecha saldría un día antes.
 */
export function fecha(iso: string) {
  return SIN_HORA.format(new Date(`${iso}T00:00:00Z`));
}

/**
 * Un instante, que la API envía siempre en UTC. Se muestra en la hora del
 * navegador: el ordenador de recepción está en el gimnasio, así que es la suya.
 */
export function fechaHora(iso: string) {
  return CON_HORA.format(new Date(iso));
}

export function soles(importe: number) {
  return SOLES.format(importe);
}
