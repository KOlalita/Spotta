# Spotta.web

Panel de recepción. React + Vite + TypeScript. Lo usa el personal con rol
`administrador`.

## Arrancar

```bash
npm install
npm run dev
```

Queda en `http://localhost:5173`. Necesita `Spotta.api` levantada. Sólo
entran las cuentas con rol `administrador`: una cuenta de socio que intente
entrar ve un aviso y su sesión se cierra.

## Configuración

```bash
cp .env.example .env.development
```

| Variable | Para qué |
|---|---|
| `VITE_API_URL` | base de la API (por defecto `http://localhost:5240`) |
| `VITE_SUPABASE_URL` | proyecto de Supabase |
| `VITE_SUPABASE_PUBLISHABLE_KEY` | clave publicable (`sb_publishable_...`) |

La clave publicable no es secreta: está pensada para ir en un cliente. La de
servicio no va aquí nunca. Supabase sólo se usa para iniciar y cerrar sesión;
los datos se piden siempre a la API.

## Estructura

Un módulo por carpeta, como en la API:

- `api/` — el cliente HTTP, único sitio que añade el token, y la traducción de
  cada código de error a una frase. Ningún código llega a la vista.
- `auth/` — sesión de Supabase, guardia de rutas y login.
- `comun/` — el marco con la barra lateral, el diálogo de confirmación y el
  formato de fechas.
- `usuarios/` — el contrato con la API, la validación del documento en vivo y
  las tres pantallas: listado, alta y ficha.

**Las fechas sin hora se formatean en UTC.** `new Date("2026-12-09")` es la
medianoche UTC, que en Lima todavía es el día 8: con la zona del navegador
saldrían un día antes. Está explicado en `comun/formato.ts`.

## Lo que viene

Según [el roadmap](../docs/roadmap.md): planes y membresías en la Fase 2, y el
escáner de QR en la 3.

El escáner necesita contexto seguro: `getUserMedia` no funciona sobre HTTP en
red local. En `localhost` va; para probar desde otra máquina hace falta HTTPS.

## Base clara

El panel va en base clara aunque la aplicación móvil sea oscura: recepción pasa
ocho horas leyendo tablas. La excepción es la pantalla de escaneo, que es oscura
porque funciona como expositor en el mostrador. Está en
[docs/diseno.md](../docs/diseno.md).
