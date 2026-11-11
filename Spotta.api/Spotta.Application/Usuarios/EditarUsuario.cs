using Microsoft.EntityFrameworkCore;
using Spotta.Application.Comun;
using Spotta.Infrastructure;
using Spotta.Infrastructure.Supabase;
using Spotta.Infrastructure.Usuarios;

namespace Spotta.Application.Usuarios;

public sealed record EditarUsuarioSolicitud(string Nombre, string Apellidos, string Dni, string Email);

/// <summary>
/// Sólo un administrador cambia los datos personales (RN-41). El rol no se
/// edita: se asigna al crear la cuenta (RN-07). Cambiar el documento no cambia
/// la contraseña, que sólo era el DNI al principio (RN-04).
/// </summary>
public sealed class EditarUsuario(SpottaDb db, ClienteAuthSupabase auth)
{
    public async Task EjecutarAsync(Guid id, EditarUsuarioSolicitud solicitud, Guid autorId, CancellationToken ct)
    {
        var autor = await PerfilActivo.CargarAsync(db, autorId, ct);
        var datos = DatosPersonales.Validar(solicitud.Nombre, solicitud.Apellidos, solicitud.Dni, solicitud.Email);

        var perfil = await db.Perfiles.FirstOrDefaultAsync(p => p.Id == id && p.GimnasioId == autor.GimnasioId, ct)
                     ?? throw ErroresUsuarios.NoEncontrado();

        if (datos.Dni != perfil.Dni && await db.Perfiles.AnyAsync(
                p => p.GimnasioId == perfil.GimnasioId && p.Dni == datos.Dni && p.Id != id, ct))
            throw ErroresUsuarios.DniDuplicado();

        // El correo es el usuario con el que se inicia sesión: vive en Supabase
        var correoAnterior = perfil.Email;
        var cambiaCorreo = datos.Email != correoAnterior;
        if (cambiaCorreo)
        {
            try
            {
                await auth.CambiarEmailAsync(id, datos.Email, ct);
            }
            catch (ErrorDeSupabase e) when (e.Codigo == "email_exists")
            {
                throw ErroresUsuarios.EmailDuplicado();
            }
        }

        perfil.Nombre = datos.Nombre;
        perfil.Apellidos = datos.Apellidos;
        perfil.Dni = datos.Dni;
        perfil.Email = datos.Email;

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            if (cambiaCorreo)
                await Compensacion.DeshacerAsync(
                    () => auth.CambiarEmailAsync(id, correoAnterior, CancellationToken.None), ex,
                    $"la cuenta {id} inicia sesión con {datos.Email} pero el perfil dice {correoAnterior}");

            if (ViolacionDeUnicidad.Es(ex, PerfilConfiguracion.IndiceDniUnico))
                throw ErroresUsuarios.DniDuplicado();
            throw;
        }
    }
}
