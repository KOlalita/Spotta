import { createMaterialTopTabNavigator } from "@react-navigation/material-top-tabs";
import { DarkTheme, NavigationContainer } from "@react-navigation/native";
import { StatusBar } from "expo-status-bar";
import { SafeAreaProvider } from "react-native-safe-area-context";
import { BarraSpotta } from "./src/BarraSpotta";
import { Avisos, Entreno, Inicio, Pase, Perfil } from "./src/pantallas";
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
  return (
    <SafeAreaProvider>
      <StatusBar style="light" />
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
          <Pestanas.Screen name="Perfil" component={Perfil} />
        </Pestanas.Navigator>
      </NavigationContainer>
    </SafeAreaProvider>
  );
}
