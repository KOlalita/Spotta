import { ErrorApi } from "./cliente";

/**
 * Ningún código de error a la vista (diseño, sección 7): cada código de la API
 * tiene aquí su frase. Un código nuevo sin traducir cae en el mensaje genérico,
 * nunca en el código en bruto.
 */
const MENSAJES: Record<string, string> = {
  sin_conexion: "Sin conexión con el servidor.",
  sesion_caducada: "Sesión caducada.",
  usuario_de_baja: "Cuenta dada de baja.",
  perfil_inexistente: "Cuenta sin perfil en Spotta.",
  peticion_mal_formada: "Datos con formato no válido.",

  dato_obligatorio: "Faltan campos obligatorios.",
  dni_invalido: "DNI no válido: 8 dígitos (9 si es carné de extranjería).",
  email_invalido: "Correo no válido.",
  dni_duplicado: "DNI ya registrado.",
  email_duplicado: "Correo ya registrado.",
  usuario_no_encontrado: "Socio no encontrado.",
  membresia_vigente_existente: "Membresía vigente: no se puede dar de baja hasta que termine.",
  baja_propia: "No se puede dar de baja la propia cuenta.",
  cursor_invalido: "La lista cambió. Recarga la página.",
  estado_desconocido: "Filtro no válido.",
};

const GENERICO = "Error inesperado. Inténtalo de nuevo.";

export function mensajeDe(error: unknown) {
  return error instanceof ErrorApi ? (MENSAJES[error.codigo] ?? GENERICO) : GENERICO;
}

export function codigoDe(error: unknown) {
  return error instanceof ErrorApi ? error.codigo : null;
}
