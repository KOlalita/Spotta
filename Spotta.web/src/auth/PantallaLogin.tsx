import { useEffect, useState, type FormEvent } from "react";
import { Navigate, useLocation } from "react-router";
import { esAdministrador, useSesion } from "./sesion";
import { supabase } from "./supabase";

const NO_ES_RECEPCION =
  "Esta cuenta es de socio. El panel es sólo para recepción; los socios entran por la aplicación móvil.";

/** Códigos de Supabase Auth, traducidos. Nunca se enseña el mensaje original. */
function mensajeDeLogin(codigo: string | undefined) {
  switch (codigo) {
    case "invalid_credentials":
      return "Correo o contraseña incorrectos.";
    case "user_banned":
      return "Esta cuenta está dada de baja.";
    default:
      return "No se pudo iniciar sesión. Inténtalo de nuevo.";
  }
}

export function PantallaLogin() {
  const sesion = useSesion();
  const ubicacion = useLocation();
  const [email, setEmail] = useState("");
  const [clave, setClave] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [enviando, setEnviando] = useState(false);

  // Una sesión de socio guardada en este navegador no sirve aquí: se cierra
  const esSocio = sesion.fase === "dentro" && !sesion.esRecepcion;
  useEffect(() => {
    if (esSocio) void supabase.auth.signOut();
  }, [esSocio]);

  if (sesion.fase === "dentro" && sesion.esRecepcion) {
    const desde = (ubicacion.state as { desde?: string } | null)?.desde ?? "/socios";
    return <Navigate to={desde} replace />;
  }

  async function entrar(e: FormEvent) {
    e.preventDefault();
    setEnviando(true);
    setError(null);

    const { data, error } = await supabase.auth.signInWithPassword({ email: email.trim(), password: clave });

    setEnviando(false);
    if (error) {
      setError(mensajeDeLogin(error.code));
    } else if (!esAdministrador(data)) {
      setError(NO_ES_RECEPCION);
    }
    // Si es recepción, el cambio de sesión vuelve a pintar esta pantalla y redirige
  }

  return (
    <main className="acceso">
      <section className="tarjeta acceso-tarjeta">
        <div className="marca">
          <span className="marca-punto" />
          Spotta
        </div>
        <p className="sub">Panel de recepción</p>

        <form onSubmit={entrar}>
          <label className="campo">
            <span>Correo</span>
            <input
              type="email"
              autoComplete="username"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
              autoFocus
            />
          </label>

          <label className="campo">
            <span>Contraseña</span>
            <input
              type="password"
              autoComplete="current-password"
              value={clave}
              onChange={(e) => setClave(e.target.value)}
              required
            />
            <small className="ayuda">Si no la has cambiado, es tu número de documento.</small>
          </label>

          {(error ?? (esSocio ? NO_ES_RECEPCION : null)) && (
            <p className="alerta" role="alert">
              {error ?? NO_ES_RECEPCION}
            </p>
          )}

          <button className="boton primario" disabled={enviando}>
            {enviando ? "Entrando…" : "Entrar"}
          </button>
        </form>
      </section>
    </main>
  );
}
