/**
 * Los tokens de docs/diseno.md. Tinta casi negra, lima eléctrica como único
 * acento y oliva para las superficies grandes.
 */
export const tema = {
  ink900: "#0B0E0C",
  ink800: "#141814",
  ink700: "#1E231E",
  ink600: "#2A302A",

  textoAlto: "#F4F6F2",
  textoMedio: "#A3ABA2",
  textoBajo: "#6E776D",

  lima: "#D4FF3F",
  limaPulsada: "#BFE838",
  oliva: "#6A8019",

  aviso: "#F5A524",
  peligro: "#E5484D",
} as const;

export const barra = {
  alto: 64,
  margenLateral: 16,
  margenInferior: 12,
  radio: 28,
  /** Lo que el círculo central sobresale por encima del borde superior. */
  asomaCirculo: 22,
  diametroCirculo: 64,
  /** Anillo del color del fondo que recorta el círculo contra la barra. */
  anilloSeparacion: 6,
} as const;
