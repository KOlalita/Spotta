/*
 * Espejo de Spotta.Domain.Usuarios.Documento, para validar mientras se escribe
 * (RN-03). La API vuelve a validar: esto es comodidad para recepción, no
 * seguridad. Si cambian las reglas allí, cambian aquí.
 */

const SEPARADORES = /[ .-]/g;

export type LecturaDeDocumento =
  | { valido: true; tipo: "DNI" | "Carné de extranjería" }
  | { valido: false; motivo: string };

export function leerDocumento(valor: string): LecturaDeDocumento | null {
  const limpio = valor.replace(SEPARADORES, "");
  if (limpio.length === 0) return null;

  // Las letras no se quitan: un «12345678A» es un error de tecleo, no un DNI
  if (!/^\d+$/.test(limpio)) return { valido: false, motivo: "Sólo puede llevar números" };

  if (limpio.length === 8) return { valido: true, tipo: "DNI" };
  if (limpio.length === 9) return { valido: true, tipo: "Carné de extranjería" };

  if (limpio.length < 8) {
    const faltan = 8 - limpio.length;
    return { valido: false, motivo: `Falta${faltan === 1 ? "" : "n"} ${faltan} dígito${faltan === 1 ? "" : "s"}` };
  }
  return { valido: false, motivo: "Demasiados dígitos" };
}

export function documentoValido(dni: string) {
  return leerDocumento(dni)?.valido === true;
}
