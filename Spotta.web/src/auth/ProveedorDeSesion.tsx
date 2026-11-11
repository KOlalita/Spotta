import type { Session } from "@supabase/supabase-js";
import { useEffect, useState, type ReactNode } from "react";
import { ContextoDeSesion, esAdministrador, type EstadoDeSesion } from "./sesion";
import { supabase } from "./supabase";

/** Supabase guarda la sesión en el navegador y la renueva sola; aquí sólo se escucha. */
export function ProveedorDeSesion({ children }: { children: ReactNode }) {
  const [estado, setEstado] = useState<EstadoDeSesion>({ fase: "comprobando" });

  useEffect(() => {
    const aplicar = (sesion: Session | null) =>
      setEstado(sesion ? { fase: "dentro", sesion, esRecepcion: esAdministrador(sesion) } : { fase: "fuera" });

    void supabase.auth.getSession().then(({ data }) => aplicar(data.session));
    const { data } = supabase.auth.onAuthStateChange((_evento, sesion) => aplicar(sesion));
    return () => data.subscription.unsubscribe();
  }, []);

  return <ContextoDeSesion.Provider value={estado}>{children}</ContextoDeSesion.Provider>;
}
