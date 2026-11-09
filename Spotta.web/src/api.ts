const BASE = (import.meta.env.VITE_API_URL as string | undefined) ?? "http://localhost:5240";

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

/**
 * Único punto por el que el panel habla con la API. Cuando entre la
 * autenticación (Fase 1), el token se añade aquí y en ningún otro sitio.
 */
export async function api<T>(ruta: string, init: RequestInit = {}): Promise<T> {
  const r = await fetch(`${BASE}${ruta}`, {
    ...init,
    headers: { "content-type": "application/json", ...init.headers },
  });

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
