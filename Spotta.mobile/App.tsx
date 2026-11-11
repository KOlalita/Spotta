import { createMaterialTopTabNavigator } from "@react-navigation/material-top-tabs";
import { DarkTheme, NavigationContainer } from "@react-navigation/native";
import { StatusBar } from "expo-status-bar";
import { View } from "react-native";
import { SafeAreaProvider } from "react-native-safe-area-context";
import { BarraSpotta } from "./src/BarraSpotta";
import { PantallaLogin } from "./src/PantallaLogin";
import { PantallaPerfil } from "./src/PantallaPerfil";
import { Avisos, Entreno, Inicio, Pase } from "./src/pantallas";
import { useSesion } from "./src/sesion";
import { tema } from "./src/tema";

/**
 * Las cinco pantallas son un carrusel horizontal: se cambia tocando la pestaña
 * o deslizando el dedo, y las dos vías mueven el mismo indicador.
 *
 * Es material-top-tabs con la barra abajo y un tabBar propio, no bottom-tabs:
 * las pestañas inferiores estándar de React Navigation no deslizan.
 */
const Pestanas = createMaterialTopTabNavigator();

const temaNavegacion = {
  ...DarkTheme,
  colors: { ...DarkTheme.colors, background: tema.ink900, card: tema.ink800, primary: tema.lima },
};

export default function App() {
  const sesion = useSesion();

  return (
    <SafeAreaProvider>
      <StatusBar style="light" />
      {sesion.fase === "comprobando" ? (
        // Un instante en el color de fondo, en vez de enseñar el login y saltar a dentro
        <View style={{ flex: 1, backgroundColor: tema.ink900 }} />
      ) : sesion.fase === "dentro" && sesion.esSocio ? (
        <NavigationContainer theme={temaNavegacion}>
          <Pestanas.Navigator
            initialRouteName="Inicio"
            tabBarPosition="bottom"
            tabBar={(props) => <BarraSpotta {...props} />}
            screenOptions={{ swipeEnabled: true, lazy: false }}
          >
            <Pestanas.Screen name="Inicio" component={Inicio} />
            <Pestanas.Screen name="Avisos" component={Avisos} />
            <Pestanas.Screen name="Pase" component={Pase} />
            <Pestanas.Screen name="Entreno" component={Entreno} />
            <Pestanas.Screen name="Perfil" component={PantallaPerfil} />
          </Pestanas.Navigator>
        </NavigationContainer>
      ) : (
        <PantallaLogin cuentaDeRecepcion={sesion.fase === "dentro"} />
      )}
    </SafeAreaProvider>
  );
}
