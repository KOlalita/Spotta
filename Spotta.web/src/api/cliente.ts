import { supabase } from "../auth/supabase";

const BASE = (import.meta.env.VITE_API_URL as string | undefined) ?? "http://localhost:5240";

/** Un rechazo de la API, con su código de negocio estable (arquitectura, sección 8). */
export class ErrorApi extends Error {
  readonly codigo: string;
  readonly estado: number;

  constructor(codigo: string, estado: number) {
    super(codigo);
    this.codigo = codigo;
    this.estado = estado;
  }
}

/**
 * Único punto por el que el panel habla con la API. El token se añade aquí y
 * en ningún otro sitio. getSession() lo renueva si está a punto de caducar.
 */
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

  if (respuesta.ok) {
    return respuesta.status === 204 ? (undefined as T) : ((await respuesta.json()) as T);
  }

  const problema = (await respuesta.json().catch(() => ({}))) as { code?: string };
  const codigo = problema.code ?? (respuesta.status === 401 ? "sesion_caducada" : "desconocido");

  // Sin sesión válida, o con la cuenta dada de baja, no queda nada que hacer
  // aquí: se cierra y la guardia de rutas lleva al login
  if (respuesta.status === 401 || codigo === "usuario_de_baja") {
    await supabase.auth.signOut();
  }

  throw new ErrorApi(codigo, respuesta.status);
}
