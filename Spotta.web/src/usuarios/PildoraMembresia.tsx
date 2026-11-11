import type { EstadoDeSocio, SituacionDeMembresia } from "./api";

const TEXTO: Record<EstadoDeSocio, string> = {
  vigente: "Vigente",
  programada: "Programada",
  vencida: "Vencida",
  sin_membresia: "Sin membresía",
};

const CLASE: Record<EstadoDeSocio, string> = {
  vigente: "ok",
  programada: "neutra",
  vencida: "peligro",
  sin_membresia: "apagada",
};

function vence(dias: number) {
  if (dias === 0) return "Vence hoy";
  if (dias === 1) return "Vence mañana";
  return `Vence en ${dias} días`;
}

/**
 * La situación del socio en una palabra. A siete días o menos del final, en
 * ámbar: es la tarea comercial de recepción (diseño, sección 6). La baja manda
 * sobre todo lo demás.
 */
export function PildoraMembresia({ situacion, deBaja }: { situacion: SituacionDeMembresia; deBaja?: boolean }) {
  if (deBaja) return <span className="pildora peligro">De baja</span>;

  const dias = situacion.diasRestantes;
  if (situacion.estado === "vigente" && dias !== null && dias <= 7) {
    return <span className="pildora aviso">{vence(dias)}</span>;
  }

  return <span className={`pildora ${CLASE[situacion.estado]}`}>{TEXTO[situacion.estado]}</span>;
}
