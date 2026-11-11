import Svg, { Circle, Path, Rect } from "react-native-svg";

/**
 * Los cinco glifos de la barra, dibujados a medida.
 *
 * Regla de docs/diseno.md: prohibido usar fuentes de iconos o paquetes
 * genéricos aquí. Son los cinco símbolos que el socio ve en cada pantalla y un
 * paquete los convierte en los mismos de otras cien aplicaciones.
 *
 * Lienzo 24 × 24, trazo 1,75, extremos y uniones redondeados, sin relleno
 * salvo los módulos del QR.
 */

type Props = { color: string; tamano?: number };

const TRAZO = 1.75;

function Lienzo({ color, tamano = 24, children }: Props & { children: React.ReactNode }) {
  return (
    <Svg
      width={tamano}
      height={tamano}
      viewBox="0 0 24 24"
      fill="none"
      stroke={color}
      strokeWidth={TRAZO}
      strokeLinecap="round"
      strokeLinejoin="round"
    >
      {children}
    </Svg>
  );
}

export function Casa(p: Props) {
  return (
    <Lienzo {...p}>
      <Path d="M3.6 10.4 12 4l8.4 6.4V19a1.5 1.5 0 0 1-1.5 1.5H5.1A1.5 1.5 0 0 1 3.6 19Z" />
    </Lienzo>
  );
}

export function Campana(p: Props) {
  return (
    <Lienzo {...p}>
      <Path d="M17.5 9.5a5.5 5.5 0 0 0-11 0c0 4.2-1.4 5.6-1.9 6.3h14.8c-.5-.7-1.9-2.1-1.9-6.3Z" />
      <Path d="M10 18.6a2.2 2.2 0 0 0 4 0" />
    </Lienzo>
  );
}

export function CodigoQr({ color, tamano = 28 }: Props) {
  return (
    <Svg
      width={tamano}
      height={tamano}
      viewBox="0 0 24 24"
      fill="none"
      stroke={color}
      strokeWidth={TRAZO}
      strokeLinecap="round"
      strokeLinejoin="round"
    >
      <Rect x={3.5} y={3.5} width={6.5} height={6.5} rx={1.2} />
      <Rect x={14} y={3.5} width={6.5} height={6.5} rx={1.2} />
      <Rect x={3.5} y={14} width={6.5} height={6.5} rx={1.2} />
      <Path d="M14 14h2.6M20.5 14v2.6M14 20.5h2.6M20.5 20.5h-1" />
    </Svg>
  );
}

export function Mancuerna(p: Props) {
  return (
    <Lienzo {...p}>
      <Path d="M7 8.5v7M4 10.2v3.6M17 8.5v7M20 10.2v3.6M7 12h10" />
    </Lienzo>
  );
}

export function Persona(p: Props) {
  return (
    <Lienzo {...p}>
      <Circle cx={12} cy={8.2} r={3.5} />
      <Path d="M4.8 20c0-3.5 3.3-5.4 7.2-5.4s7.2 1.9 7.2 5.4" />
    </Lienzo>
  );
}
