import type { ChangeEvent } from "react";
import type { DatosPersonales } from "./api";
import { leerDocumento } from "./documento";

type Props = {
  valores: DatosPersonales;
  onCambio: (valores: DatosPersonales) => void;
  deshabilitado?: boolean;
};

/** Los cuatro campos que comparten el alta y la edición (RN-02). */
export function CamposDatosPersonales({ valores, onCambio, deshabilitado }: Props) {
  const documento = leerDocumento(valores.dni);
  const cambiar = (campo: keyof DatosPersonales) => (e: ChangeEvent<HTMLInputElement>) =>
    onCambio({ ...valores, [campo]: e.target.value });

  return (
    <div className="campos">
      <label className="campo">
        <span>Nombre</span>
        <input value={valores.nombre} onChange={cambiar("nombre")} disabled={deshabilitado} required autoFocus />
      </label>

      <label className="campo">
        <span>Apellidos</span>
        <input value={valores.apellidos} onChange={cambiar("apellidos")} disabled={deshabilitado} required />
      </label>

      <label className="campo">
        <span>DNI</span>
        <input
          className="mono"
          inputMode="numeric"
          autoComplete="off"
          value={valores.dni}
          onChange={cambiar("dni")}
          disabled={deshabilitado}
          aria-invalid={documento?.valido === false}
          aria-describedby="ayuda-documento"
          required
        />
        <small
          id="ayuda-documento"
          className={documento === null ? "ayuda" : documento.valido ? "ayuda ok" : "ayuda error"}
        >
          {documento === null
            ? "8 dígitos (9 si es carné de extranjería)"
            : documento.valido
              ? `${documento.tipo} válido`
              : documento.motivo}
        </small>
      </label>

      <label className="campo">
        <span>Correo</span>
        <input type="email" value={valores.email} onChange={cambiar("email")} disabled={deshabilitado} required />
        <small className="ayuda">Usuario de inicio de sesión</small>
      </label>
    </div>
  );
}
