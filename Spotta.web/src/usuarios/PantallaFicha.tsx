import { useEffect, useState, type FormEvent } from "react";
import { Link, useLocation, useParams } from "react-router";
import { motivoLegible } from "../accesos/motivos";
import { codigoDe, mensajeDe } from "../api/errores";
import { useSesion } from "../auth/sesion";
import { Confirmar } from "../comun/Confirmar";
import { fecha, fechaHora, soles } from "../comun/formato";
import {
  darDeBaja,
  editarUsuario,
  obtenerUsuario,
  reactivar,
  type DatosPersonales,
  type FichaDeUsuario,
} from "./api";
import { CamposDatosPersonales } from "./CamposDatosPersonales";
import { documentoValido } from "./documento";
import { PildoraMembresia } from "./PildoraMembresia";

type Carga = { fase: "cargando" } | { fase: "ok"; ficha: FichaDeUsuario } | { fase: "error"; mensaje: string };

const ESTADO_MEMBRESIA = { programada: "Programada", vigente: "Vigente", finalizada: "Finalizada" } as const;
const METODO = { efectivo: "Efectivo", tarjeta: "Tarjeta", transferencia: "Transferencia" } as const;

/**
 * Ficha del socio en tres bloques: datos, membresías y accesos (diseño,
 * sección 6). La baja va aparte y al final, lejos de lo que se usa a diario.
 */
export function PantallaFicha() {
  const { id = "" } = useParams();
  const ubicacion = useLocation();
  const sesion = useSesion();
  const miId = sesion.fase === "dentro" ? sesion.sesion.user.id : null;

  const [carga, setCarga] = useState<Carga>({ fase: "cargando" });
  const [aviso, setAviso] = useState<string | null>(
    (ubicacion.state as { recienCreado?: boolean } | null)?.recienCreado
      ? "Socio creado. Ya puede entrar en la aplicación con su correo y su documento como contraseña."
      : null,
  );

  const [borrador, setBorrador] = useState<DatosPersonales | null>(null);
  const [guardando, setGuardando] = useState(false);
  const [errorEdicion, setErrorEdicion] = useState<string | null>(null);

  const [confirmando, setConfirmando] = useState<"baja" | "reactivar" | null>(null);
  const [ocupado, setOcupado] = useState(false);
  const [errorAccion, setErrorAccion] = useState<string | null>(null);

  // Si se cambia de ficha antes de que llegue la anterior, la respuesta vieja se descarta
  useEffect(() => {
    let vigente = true;
    obtenerUsuario(id)
      .then((ficha) => vigente && setCarga({ fase: "ok", ficha }))
      .catch((e) => vigente && setCarga({ fase: "error", mensaje: mensajeDe(e) }));
    return () => {
      vigente = false;
    };
  }, [id]);

  if (carga.fase !== "ok") {
    return (
      <section className="pantalla">
        <Volver />
        {carga.fase === "cargando" ? (
          <header className="cabecera">
            <span className="hueso" style={{ width: 280, height: 28 }} />
          </header>
        ) : (
          <p className="alerta">{carga.mensaje}</p>
        )}
      </section>
    );
  }

  const f = carga.ficha;
  const deBaja = f.dadoDeBajaEn !== null;
  const esMiCuenta = f.id === miId;

  async function recargar() {
    setCarga({ fase: "ok", ficha: await obtenerUsuario(f.id) });
  }

  async function guardar(e: FormEvent) {
    e.preventDefault();
    if (!borrador) return;
    if (!documentoValido(borrador.dni)) {
      setErrorEdicion("El documento debe ser un DNI de 8 dígitos o un carné de extranjería de 9.");
      return;
    }

    setGuardando(true);
    setErrorEdicion(null);
    try {
      await editarUsuario(f.id, borrador);
      setBorrador(null);
      setAviso("Datos guardados.");
      await recargar();
    } catch (err) {
      setErrorEdicion(mensajeDe(err));
    } finally {
      setGuardando(false);
    }
  }

  async function confirmar() {
    if (!confirmando) return;
    setOcupado(true);
    setErrorAccion(null);
    try {
      if (confirmando === "baja") await darDeBaja(f.id);
      else await reactivar(f.id);
      setAviso(
        confirmando === "baja"
          ? "Dado de baja. Ya no puede iniciar sesión y su pase se rechazará."
          : "Reactivado. Ya puede volver a iniciar sesión.",
      );
      setConfirmando(null);
      await recargar();
    } catch (err) {
      setConfirmando(null);
      // Con la fecha delante se entiende sin tener que ir a mirar (diseño, sección 7)
      setErrorAccion(
        codigoDe(err) === "membresia_vigente_existente" && f.membresia.fechaFin
          ? `Tiene una membresía vigente hasta el ${fecha(f.membresia.fechaFin)}. No se puede dar de baja hasta que termine.`
          : mensajeDe(err),
      );
    } finally {
      setOcupado(false);
    }
  }

  return (
    <section className="pantalla">
      <Volver />

      <header className="cabecera">
        <div>
          <h1>
            {f.nombre} {f.apellidos}
          </h1>
          <p className="sub">
            {f.rol === "administrador" ? "Recepción" : "Socio"} · alta el {fechaHora(f.creadoEn)}
          </p>
        </div>
        <PildoraMembresia situacion={f.membresia} deBaja={deBaja} />
      </header>

      {aviso && (
        <p className="confirmacion" role="status">
          {aviso}
        </p>
      )}

      <div className="bloques">
        <article className="tarjeta bloque">
          <header className="bloque-cabecera">
            <h2>Datos personales</h2>
            {!borrador && (
              <button
                type="button"
                className="boton-texto"
                onClick={() => {
                  setAviso(null);
                  setBorrador({ nombre: f.nombre, apellidos: f.apellidos, dni: f.dni, email: f.email });
                }}
              >
                Editar
              </button>
            )}
          </header>

          {borrador ? (
            <form onSubmit={guardar}>
              <CamposDatosPersonales valores={borrador} onCambio={setBorrador} deshabilitado={guardando} />
              {borrador.email.trim().toLowerCase() !== f.email && (
                <p className="nota">Al cambiar el correo cambia también el usuario con el que inicia sesión.</p>
              )}
              {errorEdicion && (
                <p className="alerta" role="alert">
                  {errorEdicion}
                </p>
              )}
              <div className="acciones">
                <button
                  type="button"
                  className="boton secundario"
                  onClick={() => {
                    setBorrador(null);
                    setErrorEdicion(null);
                  }}
                  disabled={guardando}
                >
                  Cancelar
                </button>
                <button className="boton primario" disabled={guardando}>
                  {guardando ? "Guardando…" : "Guardar"}
                </button>
              </div>
            </form>
          ) : (
            <dl className="datos">
              <dt>Documento</dt>
              <dd className="mono">{f.dni}</dd>
              <dt>Correo</dt>
              <dd>{f.email}</dd>
              <dt>Contraseña</dt>
              <dd>{f.contrasenaCambiada ? "Cambiada por el socio" : "Sigue siendo su documento"}</dd>
            </dl>
          )}
        </article>

        <article className="tarjeta bloque">
          <h2>Membresías</h2>
          {f.membresias.length === 0 ? (
            <p className="vacio">Todavía no tiene ninguna membresía.</p>
          ) : (
            <table className="tabla">
              <thead>
                <tr>
                  <th>Plan</th>
                  <th>Inicio</th>
                  <th>Fin</th>
                  <th>Estado</th>
                  <th>Cobrado</th>
                  <th>Método</th>
                </tr>
              </thead>
              <tbody>
                {f.membresias.map((m) => (
                  <tr key={m.id}>
                    <td>{m.plan}</td>
                    <td>{fecha(m.fechaInicio)}</td>
                    <td>{fecha(m.fechaFin)}</td>
                    <td>{ESTADO_MEMBRESIA[m.estado]}</td>
                    <td>{soles(m.precioCobrado)}</td>
                    <td>{METODO[m.metodoPago]}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </article>

        <article className="tarjeta bloque">
          <h2>Accesos recientes</h2>
          {f.ultimosAccesos.length === 0 ? (
            <p className="vacio">Todavía no ha entrado al gimnasio.</p>
          ) : (
            <table className="tabla">
              <thead>
                <tr>
                  <th>Cuándo</th>
                  <th>Resultado</th>
                  <th>Motivo</th>
                </tr>
              </thead>
              <tbody>
                {f.ultimosAccesos.map((a) => (
                  <tr key={a.ocurridoEn}>
                    <td>{fechaHora(a.ocurridoEn)}</td>
                    <td>
                      <span className={`pildora ${a.resultado === "permitido" ? "ok" : "peligro"}`}>
                        {a.resultado === "permitido" ? "Permitido" : "Rechazado"}
                      </span>
                      {a.manual && <span className="insignia">Manual</span>}
                    </td>
                    <td className="tenue">{motivoLegible(a.motivo)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </article>
      </div>

      {errorAccion && (
        <p className="alerta" role="alert">
          {errorAccion}
        </p>
      )}

      <div className="zona-peligro">
        {esMiCuenta ? (
          <div>
            <h2>Tu cuenta</h2>
            <p>No puedes darte de baja a ti mismo: el panel se quedaría sin nadie que pudiera deshacerlo.</p>
          </div>
        ) : deBaja ? (
          <>
            <div>
              <h2>Dado de baja</h2>
              <p>
                Desde el {fechaHora(f.dadoDeBajaEn!)}. No puede iniciar sesión y su pase se rechaza. Su historial
                se conserva.
              </p>
            </div>
            <button type="button" className="boton secundario" onClick={() => setConfirmando("reactivar")}>
              Reactivar
            </button>
          </>
        ) : (
          <>
            <div>
              <h2>Dar de baja</h2>
              <p>
                No podrá iniciar sesión y su pase se rechazará. No se borra nada: el historial de accesos y cobros
                se conserva y la baja se puede deshacer.
              </p>
            </div>
            <button type="button" className="boton peligro" onClick={() => setConfirmando("baja")}>
              Dar de baja
            </button>
          </>
        )}
      </div>

      <Confirmar
        abierto={confirmando !== null}
        titulo={confirmando === "baja" ? `¿Dar de baja a ${f.nombre}?` : `¿Reactivar a ${f.nombre}?`}
        texto={
          confirmando === "baja"
            ? "Perderá el acceso a la aplicación y al gimnasio en cuanto confirmes."
            : "Podrá volver a iniciar sesión con su contraseña de siempre."
        }
        accion={confirmando === "baja" ? "Dar de baja" : "Reactivar"}
        peligro={confirmando === "baja"}
        ocupado={ocupado}
        onConfirmar={confirmar}
        onCancelar={() => setConfirmando(null)}
      />
    </section>
  );
}

function Volver() {
  return (
    <Link to="/socios" className="volver">
      ← Socios
    </Link>
  );
}
