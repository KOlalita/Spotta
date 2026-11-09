import { useEffect, useState } from "react";
import { obtenerSalud, type Salud } from "./api";
import "./App.css";

type Estado =
  | { fase: "cargando" }
  | { fase: "ok"; salud: Salud }
  | { fase: "error"; mensaje: string };

export default function App() {
  const [estado, setEstado] = useState<Estado>({ fase: "cargando" });

  useEffect(() => {
    obtenerSalud()
      .then((salud) => setEstado({ fase: "ok", salud }))
      .catch(() => setEstado({ fase: "error", mensaje: "No se pudo contactar con la API" }));
  }, []);

  return (
    <main className="panel">
      <header>
        <h1>Spotta</h1>
        <p className="sub">Panel de recepción</p>
      </header>

      <section className="tarjeta">
        {estado.fase === "cargando" && <p className="apagado">Comprobando la API…</p>}

        {estado.fase === "error" && (
          <p className="malo">
            {estado.mensaje}. Levanta <code>Spotta.Api</code> y recarga.
          </p>
        )}

        {estado.fase === "ok" && (
          <dl>
            <dt>Servicio</dt>
            <dd>{estado.salud.servicio}</dd>
            <dt>Versión</dt>
            <dd>{estado.salud.version}</dd>
            <dt>Base de datos</dt>
            <dd className={estado.salud.baseDeDatos === "conectada" ? "bueno" : "malo"}>
              {estado.salud.baseDeDatos}
            </dd>
          </dl>
        )}
      </section>
    </main>
  );
}
