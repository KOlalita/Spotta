import { useState, type FormEvent } from "react";
import { Link, useNavigate } from "react-router";
import { mensajeDe } from "../api/errores";
import { crearUsuario, type DatosPersonales, type Rol } from "./api";
import { CamposDatosPersonales } from "./CamposDatosPersonales";
import { documentoValido } from "./documento";

/** Alta de un socio desde recepción (RN-01). No existe registro público. */
export function PantallaNuevoSocio() {
  const navegar = useNavigate();
  const [datos, setDatos] = useState<DatosPersonales>({ nombre: "", apellidos: "", dni: "", email: "" });
  const [rol, setRol] = useState<Rol>("cliente");
  const [error, setError] = useState<string | null>(null);
  const [enviando, setEnviando] = useState(false);

  async function crear(e: FormEvent) {
    e.preventDefault();
    if (!documentoValido(datos.dni)) {
      setError("DNI no válido: 8 dígitos (9 si es carné de extranjería).");
      return;
    }

    setEnviando(true);
    setError(null);
    try {
      const { id } = await crearUsuario({ ...datos, rol });
      navegar(`/socios/${id}`, { replace: true, state: { recienCreado: true } });
    } catch (err) {
      setError(mensajeDe(err));
      setEnviando(false);
    }
  }

  return (
    <section className="pantalla estrecha">
      <Link to="/socios" className="volver">
        ← Socios
      </Link>
      <header className="cabecera">
        <h1>Nuevo socio</h1>
      </header>

      <form className="tarjeta formulario" onSubmit={crear}>
        <CamposDatosPersonales valores={datos} onCambio={setDatos} deshabilitado={enviando} />

        <fieldset className="grupo">
          <legend>Tipo de cuenta</legend>
          <label className="opcion">
            <input type="radio" name="rol" checked={rol === "cliente"} onChange={() => setRol("cliente")} />
            Socio
            <small>Aplicación móvil</small>
          </label>
          <label className="opcion">
            <input
              type="radio"
              name="rol"
              checked={rol === "administrador"}
              onChange={() => setRol("administrador")}
            />
            Recepción
            <small>Panel web</small>
          </label>
        </fieldset>

        <p className="nota">
          Contraseña inicial: DNI.
        </p>

        {error && (
          <p className="alerta" role="alert">
            {error}
          </p>
        )}

        <div className="acciones">
          <Link to="/socios" className="boton secundario">
            Cancelar
          </Link>
          <button className="boton primario" disabled={enviando}>
            {enviando ? "Creando…" : "Crear socio"}
          </button>
        </div>
      </form>
    </section>
  );
}
