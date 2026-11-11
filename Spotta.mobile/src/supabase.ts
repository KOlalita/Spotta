import "react-native-url-polyfill/auto";
import AsyncStorage from "@react-native-async-storage/async-storage";
import { createClient } from "@supabase/supabase-js";
import { AppState, Platform } from "react-native";

const url = process.env.EXPO_PUBLIC_SUPABASE_URL;
const clave = process.env.EXPO_PUBLIC_SUPABASE_PUBLISHABLE_KEY;

if (!url || !clave) {
  throw new Error("Faltan EXPO_PUBLIC_SUPABASE_URL o EXPO_PUBLIC_SUPABASE_PUBLISHABLE_KEY en .env");
}

/**
 * Sólo para iniciar y cerrar sesión. Los datos se piden a la API, nunca a
 * Supabase: RLS está activado sin políticas y negaría cualquier consulta.
 *
 * La sesión se guarda en el teléfono y sobrevive a cerrar la aplicación. En el
 * navegador (react-native-web) se usa su almacenamiento por defecto.
 */
export const supabase = createClient(url, clave, {
  auth: {
    ...(Platform.OS !== "web" ? { storage: AsyncStorage } : {}),
    autoRefreshToken: true,
    persistSession: true,
    detectSessionInUrl: false,
  },
});

// El token se renueva sólo mientras la aplicación está en primer plano: en
// segundo plano el sistema congela los temporizadores y la renovación fallaría
if (Platform.OS !== "web") {
  AppState.addEventListener("change", (estado) => {
    if (estado === "active") void supabase.auth.startAutoRefresh();
    else void supabase.auth.stopAutoRefresh();
  });
}
