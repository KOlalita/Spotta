import { useEffect, useRef } from "react";

type Props = {
  abierto: boolean;
  titulo: string;
  texto: string;
  accion: string;
  peligro?: boolean;
  ocupado?: boolean;
  onConfirmar: () => void;
  onCancelar: () => void;
};

/**
 * Confirmación modal sobre el <dialog> del navegador, que ya resuelve el foco
 * atrapado y la tecla Escape. El botón por defecto es Cancelar: un Enter
 * despistado no debe dar de baja a nadie.
 */
export function Confirmar({ abierto, titulo, texto, accion, peligro, ocupado, onConfirmar, onCancelar }: Props) {
  const ref = useRef<HTMLDialogElement>(null);

  useEffect(() => {
    const dialogo = ref.current;
    if (!dialogo) return;
    if (abierto && !dialogo.open) dialogo.showModal();
    if (!abierto && dialogo.open) dialogo.close();
  }, [abierto]);

  return (
    <dialog
      ref={ref}
      className="dialogo"
      onCancel={(e) => {
        e.preventDefault();
        if (!ocupado) onCancelar();
      }}
    >
      <h2>{titulo}</h2>
      <p>{texto}</p>
      <div className="acciones">
        <button type="button" className="boton secundario" onClick={onCancelar} disabled={ocupado} autoFocus>
          Cancelar
        </button>
        <button
          type="button"
          className={`boton ${peligro ? "peligro" : "primario"}`}
          onClick={onConfirmar}
          disabled={ocupado}
        >
          {ocupado ? "Un momento…" : accion}
        </button>
      </div>
    </dialog>
  );
}
