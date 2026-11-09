/**
 * Único punto por el que la aplicación habla con la API.
 *
 * En React Native no hay CORS, ni cookies, ni mismo origen: es un cliente HTTP
 * plano. El token irá en la cabecera Authorization cuando entre la Fase 1.
 */
const BASE = process.env.EXPO_PUBLIC_API_URL ?? "http://localhost:5240";

export class ErrorApi extends Error {
  readonly code: string;
  readonly status: number;

  constructor(code: string, status: number) {
    super(code);
    this.code = code;
    this.status = status;
  }

  static async desde(r: Response): Promise<ErrorApi> {
    const problema = await r.json().catch(() => ({}));
    return new ErrorApi(problema.code ?? "desconocido", r.status);
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
  const r = await fetch(`${BASE}${ruta}`, {
    ...init,
    headers: { "content-type": "application/json", ...init.headers },
  });

  const fecha = r.headers.get("date");
  if (fecha) {
    const servidor = Date.parse(fecha);
    if (!Number.isNaN(servidor)) desfaseReloj = servidor - Date.now();
  }

  if (!r.ok) throw await ErrorApi.desde(r);
  return r.status === 204 ? (undefined as T) : ((await r.json()) as T);
}

export type Salud = {
  servicio: string;
  version: string;
  baseDeDatos: string;
  ahora: string;
};

export const obtenerSalud = () => api<Salud>("/api/salud");
