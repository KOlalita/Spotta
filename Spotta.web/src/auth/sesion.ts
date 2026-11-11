import type { Session } from "@supabase/supabase-js";
import { createContext, useContext } from "react";

export type EstadoDeSesion =
  | { fase: "comprobando" }
  | { fase: "fuera" }
  | { fase: "dentro"; sesion: Session; esRecepcion: boolean };

export const ContextoDeSesion = createContext<EstadoDeSesion>({ fase: "comprobando" });

export function useSesion() {
  return useContext(ContextoDeSesion);
}

/**
 * El rol viaja en app_metadata, que sólo escribe la API con la clave de
 * servicio (RN-07). Aquí decide qué pantalla se enseña; quien lo hace cumplir
 * de verdad es la API, que comprueba el token en cada petición.
 */
export function esAdministrador(sesion: Pick<Session, "user">) {
  return (sesion.user.app_metadata as { role?: string }).role === "administrador";
}
