import { NavLink, Outlet } from "react-router";
import { useSesion } from "../auth/sesion";
import { supabase } from "../auth/supabase";

/** Barra lateral fija y contenido (diseño, sección 6). Sólo aparece lo que ya funciona. */
export function Marco() {
  const sesion = useSesion();
  const correo = sesion.fase === "dentro" ? sesion.sesion.user.email : "";

  return (
    <div className="marco">
      <aside className="lateral">
        <div className="marca">
          <span className="marca-punto" />
          Spotta
        </div>

        <nav aria-label="Secciones">
          <NavLink to="/socios" className="enlace">
            Socios
          </NavLink>
        </nav>

        <div className="lateral-pie">
          <span className="lateral-cuenta" title={correo}>
            {correo}
          </span>
          <button type="button" className="boton-texto" onClick={() => void supabase.auth.signOut()}>
            Cerrar sesión
          </button>
        </div>
      </aside>

      <main className="contenido">
        <Outlet />
      </main>
    </div>
  );
}
