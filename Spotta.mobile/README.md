# Spotta.mobile

Aplicación del socio. React Native + Expo (SDK 58) + TypeScript.

Sólo teléfono, sólo vertical. No hay disposición para tableta ni horizontal.

## Arrancar

```bash
npm install
npx expo start
```

## Configuración

```bash
cp .env.example .env
```

| Variable | Para qué |
|---|---|
| `EXPO_PUBLIC_API_URL` | base de la API |

**En un dispositivo real, `localhost` es el propio teléfono.** Hay que poner la
IP de tu máquina en la red local (`http://192.168.1.x:5240`) o levantar un túnel.
En el emulador sí funciona `localhost`.

## Navegación

Cinco posiciones, con el pase en el centro y elevado:

```
Inicio · Avisos · [Pase] · Entreno · Perfil
```

Es `@react-navigation/material-top-tabs` con `tabBarPosition="bottom"` y una
barra propia, **no** `bottom-tabs`: las pestañas inferiores estándar no deslizan,
y aquí el gesto es parte del diseño.

Dos cosas del montaje que no son caprichos:

**El círculo central no desborda la barra.** En Android lo que sobresale del
contenedor no recibe toques, así que el contenedor se extiende hacia arriba en
transparente y el círculo vive dentro de sus límites.

**El margen inferior sale de `useSafeAreaInsets`**, no de una constante: en los
teléfonos con indicador de inicio, una barra pegada al borde queda debajo del
gesto del sistema.

## Glifos

Los cinco símbolos de la barra están dibujados a medida en `src/glifos.tsx`.
**No se usan fuentes de iconos ni paquetes genéricos en la barra** — Tabler,
Font Awesome, Ionicons, Material, Feather, Lucide. Es la regla de
[docs/diseno.md](../docs/diseno.md): son los cinco símbolos que el socio ve en
cada pantalla, y un paquete los convierte en los mismos de otras cien
aplicaciones.

Fuera de la barra, en listas y formularios, sí se puede usar un paquete.

## Estructura

- `src/tema.ts` — tokens de color y medidas de la barra
- `src/glifos.tsx` — los cinco glifos
- `src/BarraSpotta.tsx` — la barra y el botón central
- `src/pantallas.tsx` — las cinco pantallas
- `src/api.ts` — cliente HTTP y corrección del reloj

En React Native no hay CORS, ni cookies, ni mismo origen: `src/api.ts` es un
cliente HTTP plano y el token irá en la cabecera `Authorization`.

`src/api.ts` ya guarda el desfase entre el reloj del servidor y el del teléfono
leyendo la cabecera `Date` de cada respuesta. Lo necesita el pase TOTP de la
Fase 3, que depende de la hora.

## Avisos sobre el proyecto generado

La plantilla de Expo trae `AGENTS.md` y `.claude/settings.json`, que no son del
proyecto sino del andamiaje. El `LICENSE` del template (copyright de Expo) se
quitó.

`npm audit` señala un aviso en `minimatch`, una dependencia de la herramienta de
Expo. El «arreglo» automático propone bajar a Expo 53; no se aplica.
