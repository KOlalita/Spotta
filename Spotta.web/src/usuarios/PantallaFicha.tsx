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
  restablecerContrasena,
  type DatosPersonales,
  type FichaDeUsuario,
} from "./api";
import { CamposDatosPersonales } from "./CamposDatosPersonales";
import { documentoValido } from "./documento";
import { PildoraMembresia } from "./PildoraMembresia";

type Accion = "baja" | "reactivar" | "restablecer";

type Carga = { fase: "cargando" } | { fase: "ok"; ficha: FichaDeUsuario } | { fase: "error"; mensaje: string };

const EJECUTAR: Record<Accion, (id: string) => Promise<void>> = {
  baja: darDeBaja,
  reactivar,
  restablecer: restablecerContrasena,
};

const TEXTOS: Record<Accion, { titulo: string; texto: string; accion: string; hecho: string }> = {
  baja: {
    titulo: "¿Dar de baja a",
    texto: "Se bloqueará el acceso a la aplicación y al gimnasio.",
    accion: "Dar de baja",
    hecho: "Socio dado de baja.",
  },
  reactivar: {
    titulo: "¿Reactivar a",
    texto: "Se restablecerá el acceso con su contraseña actual.",
    accion: "Reactivar",
    hecho: "Socio reactivado.",
  },
  // Es la recuperación de contraseña sin correo: el socio la olvidó y viene a recepción
  restablecer: {
    titulo: "¿Restablecer la contraseña de",
    texto: "",
    accion: "Restablecer",
    hecho: "Contraseña restablecida: DNI.",
  },
};

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
      ? "Socio creado. Contraseña inicial: DNI."
      : null,
  );

  const [borrador, setBorrador] = useState<DatosPersonales | null>(null);
  const [guardando, setGuardando] = useState(false);
  const [errorEdicion, setErrorEdicion] = useState<string | null>(null);

  const [confirmando, setConfirmando] = useState<Accion | null>(null);
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
      setErrorEdicion("DNI no válido: 8 dígitos (9 si es carné de extranjería).");
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
      await EJECUTAR[confirmando](f.id);
      setAviso(TEXTOS[confirmando].hecho);
      setConfirmando(null);
      await recargar();
    } catch (err) {
      setConfirmando(null);
      // Con la fecha delante se entiende sin tener que ir a mirar (diseño, sección 7)
      setErrorAccion(
        codigoDe(err) === "membresia_vigente_existente" && f.membresia.fechaFin
          ? `Membresía vigente hasta el ${fecha(f.membresia.fechaFin)}: no se puede dar de baja hasta que termine.`
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
                <p className="nota">El correo es también el usuario de inicio de sesión.</p>
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
              <dt>DNI</dt>
              <dd className="mono">{f.dni}</dd>
              <dt>Correo</dt>
              <dd>{f.email}</dd>
              <dt>Contraseña</dt>
              <dd className="dato-con-accion">
                {f.contrasenaCambiada ? "Personalizada" : "DNI"}
                <button type="button" className="boton-texto" onClick={() => setConfirmando("restablecer")}>
                  Restablecer
                </button>
              </dd>
            </dl>
          )}
        </article>

        <article className="tarjeta bloque">
          <h2>Membresías</h2>
          {f.membresias.length === 0 ? (
            <p className="vacio">Sin membresías encontradas.</p>
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
            <p className="vacio">Sin accesos registrados.</p>
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
            <h2>Cuenta propia</h2>
            <p>No se puede dar de baja la propia cuenta.</p>
          </div>
        ) : deBaja ? (
          <>
            <div>
              <h2>Dado de baja</h2>
              <p>
                Desde el {fechaHora(f.dadoDeBajaEn!)}. Sin acceso a la aplicación ni al gimnasio.
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
                Bloquea el acceso a la aplicación y al gimnasio. El historial se conserva y la baja es
                reversible.
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
        titulo={confirmando ? `${TEXTOS[confirmando].titulo} ${f.nombre}?` : ""}
        texto={confirmando === "restablecer" ? `La contraseña pasa a ser su DNI: ${f.dni}.` : confirmando ? TEXTOS[confirmando].texto : ""}
        accion={confirmando ? TEXTOS[confirmando].accion : ""}
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
