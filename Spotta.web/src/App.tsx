import { BrowserRouter, Navigate, Route, Routes } from "react-router";
import { PantallaLogin } from "./auth/PantallaLogin";
import { RutaDeRecepcion } from "./auth/RutaDeRecepcion";
import { ProveedorDeSesion } from "./auth/ProveedorDeSesion";
import { Marco } from "./comun/Marco";
import { PantallaFicha } from "./usuarios/PantallaFicha";
import { PantallaNuevoSocio } from "./usuarios/PantallaNuevoSocio";
import { PantallaSocios } from "./usuarios/PantallaSocios";

/** El mapa del panel. Todo lo que no es el login exige una sesión de recepción. */
export default function App() {
  return (
    <ProveedorDeSesion>
      <BrowserRouter>
        <Routes>
          <Route path="/login" element={<PantallaLogin />} />

          <Route element={<RutaDeRecepcion />}>
            <Route element={<Marco />}>
              <Route path="/socios" element={<PantallaSocios />} />
              <Route path="/socios/nuevo" element={<PantallaNuevoSocio />} />
              <Route path="/socios/:id" element={<PantallaFicha />} />
            </Route>
          </Route>

          <Route path="*" element={<Navigate to="/socios" replace />} />
        </Routes>
      </BrowserRouter>
    </ProveedorDeSesion>
  );
}
