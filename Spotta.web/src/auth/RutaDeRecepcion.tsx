import { Navigate, Outlet, useLocation } from "react-router";
import { useSesion } from "./sesion";

/**
 * Guardia de todo el panel. Sin sesión, al login recordando adónde iba. Con
 * sesión de socio, también al login, que la cierra y explica por qué.
 */
export function RutaDeRecepcion() {
  const sesion = useSesion();
  const ubicacion = useLocation();

  if (sesion.fase === "comprobando") return null;

  if (sesion.fase === "fuera" || !sesion.esRecepcion) {
    return <Navigate to="/login" replace state={{ desde: ubicacion.pathname }} />;
  }

  return <Outlet />;
}
