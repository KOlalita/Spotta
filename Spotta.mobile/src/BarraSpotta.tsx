import type { MaterialTopTabBarProps } from "@react-navigation/material-top-tabs";
import { Animated, Pressable, StyleSheet, Text, View } from "react-native";
import { useSafeAreaInsets } from "react-native-safe-area-context";
import { Campana, Casa, CodigoQr, Mancuerna, Persona } from "./glifos";
import { barra, tema } from "./tema";

type Glifo = (p: { color: string; tamano?: number }) => React.ReactElement;

const PESTANAS: { nombre: string; etiqueta: string; glifo: Glifo }[] = [
  { nombre: "Inicio", etiqueta: "Inicio", glifo: Casa },
  { nombre: "Avisos", etiqueta: "Avisos", glifo: Campana },
  { nombre: "Pase", etiqueta: "", glifo: CodigoQr },
  { nombre: "Entreno", etiqueta: "Entreno", glifo: Mancuerna },
  { nombre: "Perfil", etiqueta: "Perfil", glifo: Persona },
];

const CENTRO = 2;
const RESERVADA = 3; // Entreno: nunca se ilumina, no hay nada activo que señalar

export function BarraSpotta({ state, navigation, position }: MaterialTopTabBarProps) {
  const inferior = useSafeAreaInsets().bottom;

  return (
    // El contenedor se extiende hacia arriba en transparente: en Android lo que
    // sobresale del padre no recibe toques, así que el círculo vive aquí dentro.
    <View
      style={[
        e.contenedor,
        { paddingTop: barra.asomaCirculo + 6, paddingBottom: inferior + barra.margenInferior },
      ]}
      pointerEvents="box-none"
    >
      <View style={e.barra}>
        {PESTANAS.map((pestana, i) => {
          const activa = state.index === i;

          const alPulsar = () => {
            const evento = navigation.emit({
              type: "tabPress",
              target: state.routes[i].key,
              canPreventDefault: true,
            });
            if (!activa && !evento.defaultPrevented) {
              navigation.navigate(state.routes[i].name);
            }
          };

          // Sigue el arrastre del dedo, no sólo el salto final
          const cercania = position.interpolate({
            inputRange: [i - 1, i, i + 1],
            outputRange: [0, 1, 0],
            extrapolate: "clamp",
          });

          if (i === CENTRO) {
            return (
              <BotonCentral key={pestana.nombre} cercania={cercania} alPulsar={alPulsar} />
            );
          }

          const reservada = i === RESERVADA;
          const colorBase = reservada ? tema.textoBajo : tema.textoMedio;
          const Glifo = pestana.glifo;

          return (
            <Pressable
              key={pestana.nombre}
              onPress={alPulsar}
              style={e.pestana}
              accessibilityRole="tab"
              accessibilityState={{ selected: activa }}
              accessibilityLabel={pestana.etiqueta}
            >
              <View>
                <Glifo color={colorBase} />
                <Text style={[e.etiqueta, { color: colorBase }]}>{pestana.etiqueta}</Text>
              </View>

              {!reservada && (
                <Animated.View style={[e.encima, { opacity: cercania }]} pointerEvents="none">
                  <Glifo color={tema.lima} />
                  <Text style={[e.etiqueta, { color: tema.lima }]}>{pestana.etiqueta}</Text>
                </Animated.View>
              )}
            </Pressable>
          );
        })}
      </View>
    </View>
  );
}

function BotonCentral({
  cercania,
  alPulsar,
}: {
  cercania: Animated.AnimatedInterpolation<number>;
  alPulsar: () => void;
}) {
  const escala = cercania.interpolate({ inputRange: [0, 1], outputRange: [1, 1.06] });

  return (
    <View style={e.huecoCentral} pointerEvents="box-none">
      <Pressable
        onPress={alPulsar}
        accessibilityRole="tab"
        accessibilityLabel="Pase"
        style={e.separacion}
      >
        <Animated.View style={[e.circulo, { transform: [{ scale: escala }] }]}>
          {/* El relleno es oliva, el mismo tono de la tarjeta de pase que abre.
              La lima queda para el anillo de estado, que es un trazo de 2 px. */}
          <Animated.View style={[e.anillo, { opacity: cercania }]} pointerEvents="none" />
          <CodigoQr color="#FFFFFF" />
        </Animated.View>
      </Pressable>
    </View>
  );
}

const D = barra.diametroCirculo;
const SEP = barra.anilloSeparacion;

const e = StyleSheet.create({
  contenedor: {
    backgroundColor: tema.ink900,
  },
  barra: {
    height: barra.alto,
    marginHorizontal: barra.margenLateral,
    borderRadius: barra.radio,
    backgroundColor: tema.ink800,
    flexDirection: "row",
    alignItems: "center",
  },
  pestana: {
    flex: 1,
    alignItems: "center",
    justifyContent: "center",
  },
  encima: {
    position: "absolute",
    top: 0,
    left: 0,
    right: 0,
    bottom: 0,
    alignItems: "center",
    justifyContent: "center",
  },
  etiqueta: {
    fontSize: 11,
    fontWeight: "500",
    marginTop: 4,
    textAlign: "center",
  },
  huecoCentral: {
    flex: 1,
    alignItems: "center",
  },
  separacion: {
    position: "absolute",
    top: -(barra.asomaCirculo + SEP),
    width: D + SEP * 2,
    height: D + SEP * 2,
    borderRadius: (D + SEP * 2) / 2,
    backgroundColor: tema.ink900,
    alignItems: "center",
    justifyContent: "center",
  },
  circulo: {
    width: D,
    height: D,
    borderRadius: D / 2,
    backgroundColor: tema.oliva,
    alignItems: "center",
    justifyContent: "center",
  },
  anillo: {
    position: "absolute",
    top: 0,
    left: 0,
    right: 0,
    bottom: 0,
    borderRadius: D / 2,
    borderWidth: 2,
    borderColor: tema.lima,
  },
});
