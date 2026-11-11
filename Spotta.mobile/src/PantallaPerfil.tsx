import { useEffect, useState } from "react";
import { ActivityIndicator, Platform, Pressable, ScrollView, StyleSheet, Text, View } from "react-native";
import { ErrorApi, obtenerMiPerfil, type MiPerfil } from "./api";
import { supabase } from "./supabase";
import { tema } from "./tema";

type Estado = { fase: "cargando" } | { fase: "ok"; perfil: MiPerfil } | { fase: "error"; mensaje: string };

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

/**
 * Lo que el socio ve de sí mismo (RN-40) y el cierre de sesión. Los datos no se
 * editan aquí: sólo los cambia recepción (RN-41).
 */
export function PantallaPerfil() {
  const [estado, setEstado] = useState<Estado>({ fase: "cargando" });
  const [saliendo, setSaliendo] = useState(false);

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

  return (
    <ScrollView style={e.fondo} contentContainerStyle={e.contenido}>
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
            <Fila clave="Documento" valor={estado.perfil.dni} mono />
          </View>

          {!estado.perfil.contrasenaCambiada && (
            <View style={e.aviso}>
              <Text style={e.avisoTexto}>
                Tu contraseña sigue siendo tu número de documento, que conoce cualquiera que haya visto tu
                ficha. Conviene cambiarla.
              </Text>
            </View>
          )}
        </>
      )}

      <Pressable
        onPress={salir}
        disabled={saliendo}
        accessibilityRole="button"
        style={({ pressed }) => [e.salir, pressed && e.salirPulsado]}
      >
        <Text style={e.salirTexto}>{saliendo ? "Cerrando sesión…" : "Cerrar sesión"}</Text>
      </Pressable>
    </ScrollView>
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

const e = StyleSheet.create({
  fondo: { flex: 1, backgroundColor: tema.ink900 },
  contenido: { padding: 20, paddingTop: 64, paddingBottom: 140 },
  titulo: { color: tema.textoAlto, fontSize: 28, fontWeight: "600", marginTop: 12 },
  cargando: { marginTop: 48 },
  error: { color: tema.textoMedio, fontSize: 15, marginTop: 24 },
  tarjeta: { backgroundColor: tema.ink800, borderRadius: 14, padding: 20, marginTop: 24 },
  nombre: { color: tema.textoAlto, fontSize: 19, fontWeight: "600" },
  gimnasio: { color: tema.textoMedio, fontSize: 13, marginTop: 2, marginBottom: 12 },
  fila: { paddingVertical: 10, borderTopWidth: 1, borderTopColor: tema.ink700 },
  clave: { color: tema.textoBajo, fontSize: 13, fontWeight: "500" },
  valor: { color: tema.textoAlto, fontSize: 15, marginTop: 2 },
  mono: { fontFamily: Platform.select({ ios: "Menlo", default: "monospace" }), letterSpacing: 0.5 },
  aviso: {
    marginTop: 16,
    padding: 16,
    borderRadius: 14,
    borderWidth: 1,
    borderColor: tema.aviso,
  },
  avisoTexto: { color: tema.textoAlto, fontSize: 15, lineHeight: 22 },
  salir: {
    height: 52,
    marginTop: 32,
    borderRadius: 14,
    borderWidth: 1,
    borderColor: tema.ink600,
    alignItems: "center",
    justifyContent: "center",
  },
  salirPulsado: { backgroundColor: tema.ink800 },
  salirTexto: { color: tema.textoAlto, fontSize: 16, fontWeight: "600" },
});
