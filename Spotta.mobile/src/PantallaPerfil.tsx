import { useEffect, useState } from "react";
import { ActivityIndicator, Platform, Pressable, ScrollView, StyleSheet, Text, TextInput, View } from "react-native";
import { cambiarMiContrasena, ErrorApi, obtenerMiPerfil, type MiPerfil } from "./api";
import { supabase } from "./supabase";
import { tema } from "./tema";

type Estado = { fase: "cargando" } | { fase: "ok"; perfil: MiPerfil } | { fase: "error"; mensaje: string };

const LARGO_MINIMO = 8;

const LARGA = new Intl.DateTimeFormat("es-PE", { day: "numeric", month: "long", timeZone: "UTC" });

/** Fechas en lenguaje natural en el móvil (diseño, sección 7). */
function situacion({ estado, fechaFin, diasRestantes }: MiPerfil["membresia"]) {
  const fin = fechaFin ? LARGA.format(new Date(`${fechaFin}T00:00:00Z`)) : "";
  switch (estado) {
    case "vigente":
      if (diasRestantes === 0) return "Vigente · vence hoy";
      if (diasRestantes === 1) return "Vigente · vence mañana";
      return `Vigente · quedan ${diasRestantes} días`;
    case "programada":
      return `Programada · hasta el ${fin}`;
    case "vencida":
      return `Vencida el ${fin}`;
    default:
      return "Sin membresía. Pásate por recepción.";
  }
}

/** Códigos de la API, traducidos. Nunca se enseña el código en bruto. */
function mensajeDeContrasena(error: unknown) {
  const codigo = error instanceof ErrorApi ? error.codigo : null;
  switch (codigo) {
    case "contrasena_actual_incorrecta":
      return "La contraseña actual no es correcta.";
    case "contrasena_corta":
      return `La nueva necesita al menos ${LARGO_MINIMO} caracteres.`;
    case "contrasena_es_dni":
      return "La nueva no puede ser tu DNI.";
    case "contrasena_debil":
      return "Esa contraseña es demasiado fácil. Prueba con otra.";
    case "sin_conexion":
      return "Sin conexión. Inténtalo cuando vuelva la red.";
    default:
      return "No se pudo cambiar la contraseña. Inténtalo de nuevo.";
  }
}

/**
 * Lo que el socio ve de sí mismo (RN-40), el cambio de contraseña (RN-04) y el
 * cierre de sesión. Los datos personales no se editan aquí: sólo los cambia
 * recepción (RN-41).
 */
export function PantallaPerfil() {
  const [estado, setEstado] = useState<Estado>({ fase: "cargando" });
  const [saliendo, setSaliendo] = useState(false);
  const [cambiando, setCambiando] = useState(false);
  const [hecho, setHecho] = useState(false);

  useEffect(() => {
    let vigente = true;
    obtenerMiPerfil()
      .then((perfil) => vigente && setEstado({ fase: "ok", perfil }))
      .catch(
        (e) =>
          vigente &&
          setEstado({
            fase: "error",
            mensaje:
              e instanceof ErrorApi && e.codigo === "sin_conexion"
                ? "Sin conexión. Tus datos aparecerán cuando vuelva la red."
                : "No se pudieron cargar tus datos.",
          }),
      );
    return () => {
      vigente = false;
    };
  }, []);

  async function salir() {
    setSaliendo(true);
    await supabase.auth.signOut();
  }

  function contrasenaCambiada() {
    setCambiando(false);
    setHecho(true);
    if (estado.fase === "ok") setEstado({ fase: "ok", perfil: { ...estado.perfil, contrasenaCambiada: true } });
  }

  return (
    <ScrollView style={e.fondo} contentContainerStyle={e.contenido} keyboardShouldPersistTaps="handled">
      <Text style={e.titulo}>Perfil</Text>

      {estado.fase === "cargando" && <ActivityIndicator style={e.cargando} color={tema.textoMedio} />}

      {estado.fase === "error" && <Text style={e.error}>{estado.mensaje}</Text>}

      {estado.fase === "ok" && (
        <>
          <View style={e.tarjeta}>
            <Text style={e.nombre}>
              {estado.perfil.nombre} {estado.perfil.apellidos}
            </Text>
            <Text style={e.gimnasio}>{estado.perfil.gimnasio}</Text>

            <Fila clave="Membresía" valor={situacion(estado.perfil.membresia)} />
            <Fila clave="Correo" valor={estado.perfil.email} />
            <Fila clave="DNI" valor={estado.perfil.dni} mono />
          </View>

          {!estado.perfil.contrasenaCambiada && !cambiando && (
            <View style={e.aviso}>
              <Text style={e.avisoTexto}>
                Tu contraseña sigue siendo tu DNI, que conoce cualquiera que haya visto tu ficha. Conviene
                cambiarla.
              </Text>
            </View>
          )}

          {hecho && !cambiando && <Text style={e.hecho}>Contraseña cambiada.</Text>}

          {cambiando ? (
            <FormularioContrasena
              dni={estado.perfil.dni}
              onCancelar={() => setCambiando(false)}
              onCambiada={contrasenaCambiada}
            />
          ) : (
            <Boton
              texto="Cambiar contraseña"
              onPress={() => {
                setHecho(false);
                setCambiando(true);
              }}
            />
          )}
        </>
      )}

      <Boton texto={saliendo ? "Cerrando sesión…" : "Cerrar sesión"} onPress={salir} deshabilitado={saliendo} />
    </ScrollView>
  );
}

function FormularioContrasena({
  dni,
  onCancelar,
  onCambiada,
}: {
  dni: string;
  onCancelar: () => void;
  onCambiada: () => void;
}) {
  const [actual, setActual] = useState("");
  const [nueva, setNueva] = useState("");
  const [repetida, setRepetida] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [enviando, setEnviando] = useState(false);

  async function guardar() {
    // Lo que se puede comprobar aquí se comprueba aquí: la API lo repite igual
    if (nueva.length < LARGO_MINIMO) return setError(`La nueva necesita al menos ${LARGO_MINIMO} caracteres.`);
    if (nueva === dni) return setError("La nueva no puede ser tu DNI.");
    if (nueva !== repetida) return setError("Las dos contraseñas nuevas no coinciden.");

    setEnviando(true);
    setError(null);
    try {
      await cambiarMiContrasena(actual, nueva);
      onCambiada();
    } catch (err) {
      setError(mensajeDeContrasena(err));
      setEnviando(false);
    }
  }

  return (
    <View style={e.tarjeta}>
      <Text style={e.subtitulo}>Cambiar contraseña</Text>

      <Text style={e.etiqueta}>Contraseña actual</Text>
      <TextInput
        style={e.campo}
        value={actual}
        onChangeText={setActual}
        secureTextEntry
        autoComplete="current-password"
        editable={!enviando}
      />

      <Text style={e.etiqueta}>Nueva</Text>
      <TextInput
        style={e.campo}
        value={nueva}
        onChangeText={setNueva}
        secureTextEntry
        autoComplete="new-password"
        editable={!enviando}
      />
      <Text style={e.ayuda}>Al menos {LARGO_MINIMO} caracteres, y que no sea tu DNI.</Text>

      <Text style={e.etiqueta}>Repite la nueva</Text>
      <TextInput
        style={e.campo}
        value={repetida}
        onChangeText={setRepetida}
        secureTextEntry
        autoComplete="new-password"
        onSubmitEditing={guardar}
        editable={!enviando}
      />

      {error && (
        <Text style={e.errorFormulario} accessibilityRole="alert">
          {error}
        </Text>
      )}

      <Pressable
        onPress={guardar}
        disabled={enviando}
        accessibilityRole="button"
        style={({ pressed }) => [e.primario, pressed && e.primarioPulsado, enviando && e.ocupado]}
      >
        {enviando ? <ActivityIndicator color={tema.ink900} /> : <Text style={e.primarioTexto}>Guardar</Text>}
      </Pressable>
      <Pressable onPress={onCancelar} disabled={enviando} accessibilityRole="button" style={e.cancelar}>
        <Text style={e.cancelarTexto}>Cancelar</Text>
      </Pressable>
    </View>
  );
}

function Fila({ clave, valor, mono }: { clave: string; valor: string; mono?: boolean }) {
  return (
    <View style={e.fila}>
      <Text style={e.clave}>{clave}</Text>
      <Text style={[e.valor, mono && e.mono]}>{valor}</Text>
    </View>
  );
}

function Boton({ texto, onPress, deshabilitado }: { texto: string; onPress: () => void; deshabilitado?: boolean }) {
  return (
    <Pressable
      onPress={onPress}
      disabled={deshabilitado}
      accessibilityRole="button"
      style={({ pressed }) => [e.secundario, pressed && e.secundarioPulsado]}
    >
      <Text style={e.secundarioTexto}>{texto}</Text>
    </Pressable>
  );
}

const e = StyleSheet.create({
  fondo: { flex: 1, backgroundColor: tema.ink900 },
  contenido: { padding: 20, paddingTop: 64, paddingBottom: 140 },
  titulo: { color: tema.textoAlto, fontSize: 28, fontWeight: "600", marginTop: 12 },
  cargando: { marginTop: 48 },
  error: { color: tema.textoMedio, fontSize: 15, marginTop: 24 },
  tarjeta: { backgroundColor: tema.ink800, borderRadius: 14, padding: 20, marginTop: 24 },
  nombre: { color: tema.textoAlto, fontSize: 19, fontWeight: "600" },
  subtitulo: { color: tema.textoAlto, fontSize: 17, fontWeight: "600", marginBottom: 4 },
  gimnasio: { color: tema.textoMedio, fontSize: 13, marginTop: 2, marginBottom: 12 },
  fila: { paddingVertical: 10, borderTopWidth: 1, borderTopColor: tema.ink700 },
  clave: { color: tema.textoBajo, fontSize: 13, fontWeight: "500" },
  valor: { color: tema.textoAlto, fontSize: 15, marginTop: 2 },
  mono: { fontFamily: Platform.select({ ios: "Menlo", default: "monospace" }), letterSpacing: 0.5 },
  aviso: { marginTop: 16, padding: 16, borderRadius: 14, borderWidth: 1, borderColor: tema.aviso },
  avisoTexto: { color: tema.textoAlto, fontSize: 15, lineHeight: 22 },
  hecho: { color: tema.lima, fontSize: 15, fontWeight: "600", marginTop: 16 },
  etiqueta: { color: tema.textoMedio, fontSize: 13, fontWeight: "500", marginBottom: 8, marginTop: 16 },
  campo: {
    height: 52,
    paddingHorizontal: 16,
    borderRadius: 8,
    backgroundColor: tema.ink900,
    borderWidth: 1,
    borderColor: tema.ink700,
    color: tema.textoAlto,
    fontSize: 16,
  },
  ayuda: { color: tema.textoBajo, fontSize: 13, marginTop: 8 },
  errorFormulario: { color: tema.peligro, fontSize: 15, lineHeight: 21, marginTop: 16 },
  primario: {
    height: 52,
    marginTop: 24,
    borderRadius: 14,
    backgroundColor: tema.lima,
    alignItems: "center",
    justifyContent: "center",
  },
  primarioPulsado: { backgroundColor: tema.limaPulsada },
  primarioTexto: { color: tema.ink900, fontSize: 16, fontWeight: "700" },
  ocupado: { opacity: 0.7 },
  cancelar: { height: 44, marginTop: 8, alignItems: "center", justifyContent: "center" },
  cancelarTexto: { color: tema.textoMedio, fontSize: 15, fontWeight: "500" },
  secundario: {
    height: 52,
    marginTop: 16,
    borderRadius: 14,
    borderWidth: 1,
    borderColor: tema.ink600,
    alignItems: "center",
    justifyContent: "center",
  },
  secundarioPulsado: { backgroundColor: tema.ink800 },
  secundarioTexto: { color: tema.textoAlto, fontSize: 16, fontWeight: "600" },
});
