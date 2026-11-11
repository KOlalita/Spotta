import { useEffect, useState } from "react";
import { ScrollView, StyleSheet, Text, View } from "react-native";
import { obtenerSalud, type Salud } from "./api";
import { Mancuerna } from "./navBarIcons";
import { tema } from "./tema";

/**
 * Pantallas de la Fase 0: la estructura y la navegación funcionando, con una
 * llamada real a la API. El contenido llega en sus fases.
 */

type Estado =
  | { fase: "cargando" }
  | { fase: "ok"; salud: Salud }
  | { fase: "error" };

export function Inicio() {
  const [estado, setEstado] = useState<Estado>({ fase: "cargando" });

  useEffect(() => {
    obtenerSalud()
      .then((salud) => setEstado({ fase: "ok", salud }))
      .catch(() => setEstado({ fase: "error" }));
  }, []);

  return (
    <ScrollView style={e.fondo} contentContainerStyle={e.contenido}>
      <Text style={e.titulo}>Inicio</Text>
      <Text style={e.apoyo}>Aquí irán el aforo (Fase 4) y la racha (Fase 5).</Text>

      <View style={e.tarjeta}>
        <Text style={e.etiquetaTarjeta}>Conexión con la API</Text>

        {estado.fase === "cargando" && <Text style={e.apagado}>Comprobando…</Text>}

        {estado.fase === "error" && (
          <Text style={e.malo}>Sin respuesta. ¿Está levantada Spotta.Api?</Text>
        )}

        {estado.fase === "ok" && (
          <>
            <Fila clave="Servicio" valor={estado.salud.servicio} />
            <Fila clave="Versión" valor={estado.salud.version} />
            <Fila
              clave="Base de datos"
              valor={estado.salud.baseDeDatos}
              bueno={estado.salud.baseDeDatos === "conectada"}
            />
          </>
        )}
      </View>
    </ScrollView>
  );
}

function Fila({ clave, valor, bueno }: { clave: string; valor: string; bueno?: boolean }) {
  return (
    <View style={e.fila}>
      <Text style={e.clave}>{clave}</Text>
      <Text style={[e.valor, bueno !== undefined && { color: bueno ? tema.lima : tema.peligro }]}>
        {valor}
      </Text>
    </View>
  );
}

export function Avisos() {
  return <Vacia titulo="Avisos" apoyo="La bandeja de notificaciones llega en la Fase 6." />;
}

export function Pase() {
  return <Vacia titulo="Pase" apoyo="El QR y la tarjeta de pase llegan en la Fase 3." />;
}

/**
 * Estado de espera definitivo, no una pantalla a medias: glifo apagado, una
 * línea y nada pulsable. Sin fecha prometida, que incumplirla es peor que no
 * darla.
 */
export function Entreno() {
  return (
    <View style={[e.fondo, e.centrada]}>
      <Mancuerna color={tema.ink600} tamano={64} />
      <Text style={[e.titulo, e.centrado]}>Entreno</Text>
      <Text style={[e.apoyo, e.centrado]}>Rutinas y seguimiento. Estamos construyéndolo.</Text>
    </View>
  );
}

function Vacia({ titulo, apoyo }: { titulo: string; apoyo: string }) {
  return (
    <View style={e.fondo}>
      <View style={e.contenido}>
        <Text style={e.titulo}>{titulo}</Text>
        <Text style={e.apoyo}>{apoyo}</Text>
      </View>
    </View>
  );
}

const e = StyleSheet.create({
  fondo: { flex: 1, backgroundColor: tema.ink900 },
  contenido: { padding: 20, paddingTop: 64 },
  centrada: { alignItems: "center", justifyContent: "center", padding: 32 },
  centrado: { textAlign: "center" },
  titulo: { color: tema.textoAlto, fontSize: 28, fontWeight: "600", marginTop: 12 },
  apoyo: { color: tema.textoMedio, fontSize: 15, marginTop: 6, lineHeight: 22 },
  tarjeta: {
    backgroundColor: tema.ink800,
    borderRadius: 14,
    padding: 18,
    marginTop: 28,
  },
  etiquetaTarjeta: {
    color: tema.textoBajo,
    fontSize: 13,
    fontWeight: "500",
    marginBottom: 12,
  },
  fila: { flexDirection: "row", justifyContent: "space-between", paddingVertical: 6 },
  clave: { color: tema.textoMedio, fontSize: 15 },
  valor: { color: tema.textoAlto, fontSize: 15, fontVariant: ["tabular-nums"] },
  apagado: { color: tema.textoMedio, fontSize: 15 },
  malo: { color: tema.peligro, fontSize: 15 },
});
