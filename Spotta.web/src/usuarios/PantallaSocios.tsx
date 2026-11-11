import { useEffect, useState } from "react";
import { Link, useNavigate, useSearchParams } from "react-router";
import { mensajeDe } from "../api/errores";
import { fecha, fechaHora } from "../comun/formato";
import { listarUsuarios, type UsuarioResumen } from "./api";
import { PildoraMembresia } from "./PildoraMembresia";

type Estado =
  | { fase: "cargando" }
  | { fase: "lista"; usuarios: UsuarioResumen[]; siguiente: string | null }
  | { fase: "error"; mensaje: string };

const FILTROS = [
  { valor: "", texto: "Todas las membresías" },
  { valor: "vigente", texto: "Vigente" },
  { valor: "programada", texto: "Programada" },
  { valor: "vencida", texto: "Vencida" },
  { valor: "sin_membresia", texto: "Sin membresía" },
];

/**
 * Tabla densa de socios (diseño, sección 6). La búsqueda y el filtro viven en
 * la URL: sobreviven a recargar, al botón atrás y a pasarle el enlace a otro.
 */
export function PantallaSocios() {
  const navegar = useNavigate();
  const [parametros, setParametros] = useSearchParams();
  const q = parametros.get("q") ?? "";
  const filtro = parametros.get("estado") ?? "";

  const [busqueda, setBusqueda] = useState(q);
  const [estado, setEstado] = useState<Estado>({ fase: "cargando" });
  const [cargandoMas, setCargandoMas] = useState(false);
  const [errorMas, setErrorMas] = useState<string | null>(null);

  // Lo que se escribe pasa a la URL tras una pausa, no con cada tecla
  useEffect(() => {
    const espera = setTimeout(() => {
      const limpia = busqueda.trim();
      if (limpia === q) return;
      setParametros(
        (p) => {
          if (limpia) p.set("q", limpia);
          else p.delete("q");
          return p;
        },
        { replace: true },
      );
    }, 250);
    return () => clearTimeout(espera);
  }, [busqueda, q, setParametros]);

  useEffect(() => {
    let vigente = true;
    listarUsuarios({ q, estado: filtro })
      .then((p) => vigente && setEstado({ fase: "lista", usuarios: p.usuarios, siguiente: p.siguiente }))
      .catch((e) => vigente && setEstado({ fase: "error", mensaje: mensajeDe(e) }));
    return () => {
      vigente = false;
    };
  }, [q, filtro]);

  function cambiarFiltro(valor: string) {
    setEstado({ fase: "cargando" });
    setParametros((p) => {
      if (valor) p.set("estado", valor);
      else p.delete("estado");
      return p;
    });
  }

  async function cargarMas() {
    if (estado.fase !== "lista" || !estado.siguiente) return;
    setCargandoMas(true);
    setErrorMas(null);
    try {
      const p = await listarUsuarios({ q, estado: filtro, cursor: estado.siguiente });
      setEstado({ fase: "lista", usuarios: [...estado.usuarios, ...p.usuarios], siguiente: p.siguiente });
    } catch (e) {
      setErrorMas(mensajeDe(e));
    } finally {
      setCargandoMas(false);
    }
  }

  const hayFiltro = q !== "" || filtro !== "";

  return (
    <section className="pantalla">
      <header className="cabecera">
        <div>
          <h1>Socios</h1>
          <p className="sub">Búsqueda por nombre, apellidos o documento.</p>
        </div>
        <Link to="/socios/nuevo" className="boton primario">
          Nuevo socio
        </Link>
      </header>

      <div className="filtros">
        <input
          type="search"
          className="buscador"
          placeholder="Buscar"
          aria-label="Buscar socios"
          value={busqueda}
          onChange={(e) => {
            setBusqueda(e.target.value);
            setEstado({ fase: "cargando" });
          }}
        />
        <select value={filtro} onChange={(e) => cambiarFiltro(e.target.value)} aria-label="Filtrar por membresía">
          {FILTROS.map((f) => (
            <option key={f.valor} value={f.valor}>
              {f.texto}
            </option>
          ))}
        </select>
      </div>

      <div className="tarjeta tabla-marco">
        <table className="tabla">
          <thead>
            <tr>
              <th>Nombre</th>
              <th>Documento</th>
              <th>Membresía</th>
              <th>Fin</th>
              <th>Último acceso</th>
            </tr>
          </thead>
          <tbody>
            {estado.fase === "cargando" && <FilasEnCarga />}

            {estado.fase === "lista" &&
              estado.usuarios.map((u) => (
                <tr
                  key={u.id}
                  className={u.deBaja ? "fila de-baja" : "fila"}
                  onClick={() => navegar(`/socios/${u.id}`)}
                >
                  <td>
                    <Link to={`/socios/${u.id}`} onClick={(e) => e.stopPropagation()}>
                      {u.apellidos}, {u.nombre}
                    </Link>
                    {u.rol === "administrador" && <span className="insignia">Recepción</span>}
                  </td>
                  <td className="mono">{u.dniParcial}</td>
                  <td>
                    <PildoraMembresia situacion={u.membresia} deBaja={u.deBaja} />
                  </td>
                  <td className={u.membresia.fechaFin ? "" : "tenue"}>
                    {u.membresia.fechaFin ? fecha(u.membresia.fechaFin) : "—"}
                  </td>
                  <td className={u.ultimoAcceso ? "" : "tenue"}>
                    {u.ultimoAcceso ? fechaHora(u.ultimoAcceso) : "Nunca"}
                  </td>
                </tr>
              ))}
          </tbody>
        </table>

        {estado.fase === "lista" && estado.usuarios.length === 0 && (
          <p className="vacio">
            {hayFiltro
              ? "Ningún socio coincide con la búsqueda."
              : "Todavía no hay socios. Da de alta el primero con «Nuevo socio»."}
          </p>
        )}

        {estado.fase === "error" && <p className="vacio error">{estado.mensaje}</p>}
      </div>

      {estado.fase === "lista" && estado.siguiente && (
        <div className="mas">
          <button type="button" className="boton secundario" onClick={cargarMas} disabled={cargandoMas}>
            {cargandoMas ? "Cargando…" : "Cargar más"}
          </button>
        </div>
      )}
      {errorMas && <p className="alerta">{errorMas}</p>}
    </section>
  );
}

/** Esqueletos con el tamaño de las filas reales, para que nada salte al llegar. */
function FilasEnCarga() {
  return Array.from({ length: 6 }, (_, i) => (
    <tr key={i}>
      <td>
        <span className="hueso" style={{ width: 160 + ((i * 37) % 60) }} />
      </td>
      <td>
        <span className="hueso" style={{ width: 72 }} />
      </td>
      <td>
        <span className="hueso" style={{ width: 84 }} />
      </td>
      <td>
        <span className="hueso" style={{ width: 88 }} />
      </td>
      <td>
        <span className="hueso" style={{ width: 104 }} />
      </td>
    </tr>
  ));
}
