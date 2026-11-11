import { api } from "../api/cliente";

/*
 * El contrato del módulo de usuarios con la API: los tipos que viajan y las
 * llamadas que los traen. Los enums llegan como texto en minúsculas.
 */

export type Rol = "administrador" | "cliente";

export type EstadoDeSocio = "vigente" | "programada" | "vencida" | "sin_membresia";

export type SituacionDeMembresia = {
  estado: EstadoDeSocio;
  fechaFin: string | null;
  diasRestantes: number | null;
};

export type UsuarioResumen = {
  id: string;
  nombre: string;
  apellidos: string;
  dniParcial: string;
  email: string;
  rol: Rol;
  deBaja: boolean;
  membresia: SituacionDeMembresia;
  ultimoAcceso: string | null;
};

export type PaginaDeUsuarios = {
  usuarios: UsuarioResumen[];
  siguiente: string | null;
};

export type FichaDeUsuario = {
  id: string;
  nombre: string;
  apellidos: string;
  dni: string;
  email: string;
  rol: Rol;
  contrasenaCambiada: boolean;
  dadoDeBajaEn: string | null;
  creadoEn: string;
  membresia: SituacionDeMembresia;
  membresias: {
    id: string;
    plan: string;
    fechaInicio: string;
    fechaFin: string;
    estado: "programada" | "vigente" | "finalizada";
    precioCobrado: number;
    metodoPago: "efectivo" | "tarjeta" | "transferencia";
  }[];
  ultimosAccesos: {
    ocurridoEn: string;
    resultado: "permitido" | "rechazado";
    motivo: string | null;
    manual: boolean;
  }[];
};

export type DatosPersonales = {
  nombre: string;
  apellidos: string;
  dni: string;
  email: string;
};

export function listarUsuarios(filtro: { q?: string; estado?: string; cursor?: string | null }) {
  const parametros = new URLSearchParams();
  if (filtro.q) parametros.set("q", filtro.q);
  if (filtro.estado) parametros.set("estado", filtro.estado);
  if (filtro.cursor) parametros.set("cursor", filtro.cursor);
  return api<PaginaDeUsuarios>(`/api/usuarios?${parametros}`);
}

export const obtenerUsuario = (id: string) => api<FichaDeUsuario>(`/api/usuarios/${id}`);

export const crearUsuario = (datos: DatosPersonales & { rol: Rol }) =>
  api<{ id: string }>("/api/usuarios", { method: "POST", body: JSON.stringify(datos) });

export const editarUsuario = (id: string, datos: DatosPersonales) =>
  api<void>(`/api/usuarios/${id}`, { method: "PUT", body: JSON.stringify(datos) });

export const darDeBaja = (id: string) => api<void>(`/api/usuarios/${id}/baja`, { method: "POST" });

export const restablecerContrasena = (id: string) =>
  api<void>(`/api/usuarios/${id}/restablecer-contrasena`, { method: "POST" });

export const reactivar = (id: string) => api<void>(`/api/usuarios/${id}/reactivar`, { method: "POST" });
