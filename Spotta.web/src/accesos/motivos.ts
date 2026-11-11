/** Los motivos de rechazo de RN-23, en lenguaje llano. Nunca el código interno. */
const MOTIVOS: Record<string, string> = {
  pase_invalido: "Pase no válido",
  codigo_incorrecto: "Código incorrecto",
  pase_ya_usado: "Pase ya usado",
  dispositivo_revocado: "Dispositivo revocado",
  sin_membresia: "Sin membresía",
  membresia_vencida: "Membresía vencida",
  usuario_de_baja: "Cuenta dada de baja",
};

export function motivoLegible(codigo: string | null) {
  return codigo ? (MOTIVOS[codigo] ?? "Rechazado") : "";
}
