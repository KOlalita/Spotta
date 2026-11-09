# Spotta.web

Panel de recepción. React + Vite + TypeScript. Lo usa el personal con rol
`administrador`.

## Arrancar

```bash
npm install
npm run dev
```

Queda en `http://localhost:5173`. Necesita `Spotta.api` levantada: la pantalla
inicial consulta `/api/salud` y dice si responde.

## Configuración

```bash
cp .env.example .env.development
```

| Variable | Para qué |
|---|---|
| `VITE_API_URL` | base de la API (por defecto `http://localhost:5240`) |

Los archivos `.env` no entran en el repositorio. `.env.example` sí, como
plantilla.

## Estructura

- `src/api.ts` — único punto de contacto con la API. Cuando entre la
  autenticación, el token se añade aquí y en ningún otro sitio.
- `src/App.tsx` — por ahora, la comprobación de salud.

## Lo que viene

Según [el roadmap](../docs/roadmap.md): login y gestión de socios en la Fase 1,
planes y membresías en la 2, y el escáner de QR en la 3.

El escáner necesita contexto seguro: `getUserMedia` no funciona sobre HTTP en
red local. En `localhost` va; para probar desde otra máquina hace falta HTTPS.

## Base clara

El panel va en base clara aunque la aplicación móvil sea oscura: recepción pasa
ocho horas leyendo tablas. La excepción es la pantalla de escaneo, que es oscura
porque funciona como expositor en el mostrador. Está en
[docs/diseno.md](../docs/diseno.md).
