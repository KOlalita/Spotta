import type { Session } from "@supabase/supabase-js";
import { useEffect, useState } from "react";
import { supabase } from "./supabase";

export type EstadoDeSesion =
  | { fase: "comprobando" }
  | { fase: "fuera" }
  | { fase: "dentro"; sesion: Session; esSocio: boolean };

/**
 * La sesión de Supabase, escuchada. Al abrir la aplicación se lee la que quedó
 * guardada; después llegan solos los inicios, cierres y renovaciones.
 */
export function useSesion(): EstadoDeSesion {
  const [estado, setEstado] = useState<EstadoDeSesion>({ fase: "comprobando" });

  useEffect(() => {
    const aplicar = (sesion: Session | null) =>
      setEstado(sesion ? { fase: "dentro", sesion, esSocio: esSocio(sesion) } : { fase: "fuera" });

    void supabase.auth.getSession().then(({ data }) => aplicar(data.session));
    const { data } = supabase.auth.onAuthStateChange((_evento, sesion) => aplicar(sesion));
    return () => data.subscription.unsubscribe();
  }, []);

  return estado;
}

/**
 * La aplicación es para socios (RN-07). Una cuenta de recepción entra en el
 * panel web, no aquí. Quien lo hace cumplir de verdad es la API.
 */
export function esSocio(sesion: Pick<Session, "user">) {
  return (sesion.user.app_metadata as { role?: string }).role === "cliente";
}
