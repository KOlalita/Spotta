import { supabase } from "./supabase";

/**
 * Único punto por el que la aplicación habla con la API. El token se añade
 * aquí y en ningún otro sitio.
 *
 * En React Native no hay CORS, ni cookies, ni mismo origen: es un cliente HTTP
 * plano.
 */
const BASE = process.env.EXPO_PUBLIC_API_URL ?? "http://localhost:5240";

export class ErrorApi extends Error {
  readonly codigo: string;
  readonly estado: number;

  constructor(codigo: string, estado: number) {
    super(codigo);
    this.codigo = codigo;
    this.estado = estado;
  }
}

/** Desfase entre el reloj del servidor y el del teléfono, en milisegundos. */
let desfaseReloj = 0;

/**
 * El pase TOTP depende de la hora (RN-45). Cualquier respuesta sirve para
 * corregirla: no hace falta una llamada dedicada a pedir la hora.
 */
export const ahoraCorregido = () => Date.now() + desfaseReloj;

export async function api<T>(ruta: string, init: RequestInit = {}): Promise<T> {
  const { data } = await supabase.auth.getSession();
  const token = data.session?.access_token;

  let respuesta: Response;
  try {
    respuesta = await fetch(`${BASE}${ruta}`, {
      ...init,
      headers: {
        "content-type": "application/json",
        ...(token ? { authorization: `Bearer ${token}` } : {}),
        ...init.headers,
      },
    });
  } catch {
    throw new ErrorApi("sin_conexion", 0);
  }

  const fecha = respuesta.headers.get("date");
  if (fecha) {
    const servidor = Date.parse(fecha);
    if (!Number.isNaN(servidor)) desfaseReloj = servidor - Date.now();
  }

  if (respuesta.ok) {
    return respuesta.status === 204 ? (undefined as T) : ((await respuesta.json()) as T);
  }

  const problema = (await respuesta.json().catch(() => ({}))) as { code?: string };
  const codigo = problema.code ?? (respuesta.status === 401 ? "sesion_caducada" : "desconocido");

  // Sin sesión válida, o con la cuenta dada de baja, se cierra y la aplicación
  // vuelve sola a la pantalla de entrada
  if (respuesta.status === 401 || codigo === "usuario_de_baja") {
    await supabase.auth.signOut();
  }

  throw new ErrorApi(codigo, respuesta.status);
}

export type Salud = {
  servicio: string;
  version: string;
  baseDeDatos: string;
  ahora: string;
};

export const obtenerSalud = () => api<Salud>("/api/salud");

export type MiPerfil = {
  nombre: string;
  apellidos: string;
  dni: string;
  email: string;
  contrasenaCambiada: boolean;
  gimnasio: string;
  membresia: {
    estado: "vigente" | "programada" | "vencida" | "sin_membresia";
    fechaFin: string | null;
    diasRestantes: number | null;
  };
};

export const obtenerMiPerfil = () => api<MiPerfil>("/api/me");
