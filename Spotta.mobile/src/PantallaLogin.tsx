import { useEffect, useState } from "react";
import {
  ActivityIndicator,
  KeyboardAvoidingView,
  Platform,
  Pressable,
  StyleSheet,
  Text,
  TextInput,
  View,
} from "react-native";
import { esSocio } from "./sesion";
import { supabase } from "./supabase";
import { tema } from "./tema";

const ES_RECEPCION =
  "Esta cuenta es de recepción. La aplicación es para socios; recepción entra en el panel web.";

/** Códigos de Supabase Auth, traducidos. Nunca se enseña el mensaje original. */
function mensajeDeLogin(codigo: string | undefined) {
  switch (codigo) {
    case "invalid_credentials":
      return "Correo o contraseña incorrectos.";
    case "user_banned":
      return "Tu cuenta está dada de baja. Pregunta en recepción.";
    default:
      return "No se pudo iniciar sesión. Comprueba la conexión e inténtalo de nuevo.";
  }
}

/**
 * La entrada a la aplicación. No hay registro: las cuentas las crea recepción
 * (RN-01), con el documento como contraseña inicial (RN-04).
 */
export function PantallaLogin({ cuentaDeRecepcion }: { cuentaDeRecepcion: boolean }) {
  const [email, setEmail] = useState("");
  const [clave, setClave] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [enviando, setEnviando] = useState(false);

  // Una sesión de recepción no sirve aquí: se cierra en cuanto aparece
  useEffect(() => {
    if (cuentaDeRecepcion) void supabase.auth.signOut();
  }, [cuentaDeRecepcion]);

  async function entrar() {
    if (!email.trim() || !clave) return;
    setEnviando(true);
    setError(null);

    const { data, error } = await supabase.auth.signInWithPassword({ email: email.trim(), password: clave });

    setEnviando(false);
    if (error) setError(mensajeDeLogin(error.code));
    else if (!esSocio(data)) setError(ES_RECEPCION);
  }

  const aviso = error ?? (cuentaDeRecepcion ? ES_RECEPCION : null);

  return (
    <KeyboardAvoidingView style={e.fondo} behavior={Platform.OS === "ios" ? "padding" : undefined}>
      <View style={e.contenido}>
        <View style={e.marca}>
          <View style={e.punto} />
          <Text style={e.nombre}>Spotta</Text>
        </View>
        <Text style={e.apoyo}>Entra con la cuenta que te dieron en recepción.</Text>

        <Text style={e.etiqueta}>Correo</Text>
        <TextInput
          style={e.campo}
          value={email}
          onChangeText={setEmail}
          autoCapitalize="none"
          autoComplete="email"
          keyboardType="email-address"
          textContentType="username"
          placeholderTextColor={tema.textoBajo}
          editable={!enviando}
        />

        <Text style={e.etiqueta}>Contraseña</Text>
        <TextInput
          style={e.campo}
          value={clave}
          onChangeText={setClave}
          secureTextEntry
          autoComplete="current-password"
          textContentType="password"
          onSubmitEditing={entrar}
          editable={!enviando}
        />
        <Text style={e.ayuda}>Si no la has cambiado, es tu número de documento.</Text>

        {aviso && (
          <Text style={e.error} accessibilityRole="alert">
            {aviso}
          </Text>
        )}

        <Pressable
          onPress={entrar}
          disabled={enviando}
          accessibilityRole="button"
          style={({ pressed }) => [e.boton, pressed && e.botonPulsado, enviando && e.botonOcupado]}
        >
          {enviando ? <ActivityIndicator color={tema.ink900} /> : <Text style={e.textoBoton}>Entrar</Text>}
        </Pressable>
      </View>
    </KeyboardAvoidingView>
  );
}

const e = StyleSheet.create({
  fondo: { flex: 1, backgroundColor: tema.ink900 },
  contenido: { flex: 1, justifyContent: "center", padding: 24, maxWidth: 440, width: "100%", alignSelf: "center" },
  marca: { flexDirection: "row", alignItems: "center", gap: 10 },
  punto: { width: 14, height: 14, borderRadius: 7, backgroundColor: tema.lima },
  nombre: { color: tema.textoAlto, fontSize: 28, fontWeight: "700", letterSpacing: -0.5 },
  apoyo: { color: tema.textoMedio, fontSize: 15, lineHeight: 22, marginTop: 8, marginBottom: 32 },
  etiqueta: { color: tema.textoMedio, fontSize: 13, fontWeight: "500", marginBottom: 8, marginTop: 16 },
  campo: {
    height: 52,
    paddingHorizontal: 16,
    borderRadius: 8,
    backgroundColor: tema.ink800,
    borderWidth: 1,
    borderColor: tema.ink700,
    color: tema.textoAlto,
    fontSize: 16,
  },
  ayuda: { color: tema.textoBajo, fontSize: 13, marginTop: 8 },
  error: { color: tema.peligro, fontSize: 15, lineHeight: 21, marginTop: 20 },
  boton: {
    height: 52,
    marginTop: 28,
    borderRadius: 14,
    backgroundColor: tema.lima,
    alignItems: "center",
    justifyContent: "center",
  },
  botonPulsado: { backgroundColor: tema.limaPulsada },
  botonOcupado: { opacity: 0.7 },
  textoBoton: { color: tema.ink900, fontSize: 16, fontWeight: "700" },
});
