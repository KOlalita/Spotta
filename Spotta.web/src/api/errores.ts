import { ErrorApi } from "./cliente";

/**
 * Ningún código de error a la vista (diseño, sección 7): cada código de la API
 * tiene aquí su frase. Un código nuevo sin traducir cae en el mensaje genérico,
 * nunca en el código en bruto.
 */
const MENSAJES: Record<string, string> = {
  sin_conexion: "No hay conexión con el servidor de Spotta. Comprueba la red e inténtalo de nuevo.",
  sesion_caducada: "Tu sesión ha caducado. Vuelve a entrar.",
  usuario_de_baja: "Tu cuenta está dada de baja.",
  perfil_inexistente: "Tu cuenta no tiene perfil en Spotta. Avisa al administrador.",
  peticion_mal_formada: "Algún dato no tiene el formato esperado.",

  dato_obligatorio: "Falta rellenar algún campo.",
  dni_invalido: "El documento debe ser un DNI de 8 dígitos o un carné de extranjería de 9.",
  email_invalido: "El correo no tiene un formato válido.",
  dni_duplicado: "Ya hay un socio con ese documento.",
  email_duplicado: "Ya hay una cuenta con ese correo.",
  usuario_no_encontrado: "Ese socio no existe o no es de este gimnasio.",
  membresia_vigente_existente: "Tiene una membresía vigente: no se puede dar de baja hasta que termine.",
  baja_propia: "No puedes darte de baja a ti mismo.",
  cursor_invalido: "La lista ha cambiado mientras la recorrías. Recarga la página.",
  estado_desconocido: "Ese filtro no existe.",
};

const GENERICO = "Algo salió mal. Inténtalo de nuevo.";

export function mensajeDe(error: unknown) {
  return error instanceof ErrorApi ? (MENSAJES[error.codigo] ?? GENERICO) : GENERICO;
}

export function codigoDe(error: unknown) {
  return error instanceof ErrorApi ? error.codigo : null;
}
