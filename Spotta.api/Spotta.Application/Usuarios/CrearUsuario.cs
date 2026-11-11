using Microsoft.EntityFrameworkCore;
using Spotta.Application.Comun;
using Spotta.Domain.Usuarios;
using Spotta.Infrastructure;
using Spotta.Infrastructure.Supabase;
using Spotta.Infrastructure.Usuarios;

namespace Spotta.Application.Usuarios;

public sealed record CrearUsuarioSolicitud(
    string Nombre,
    string Apellidos,
    string Dni,
    string Email,
    Rol Rol = Rol.Cliente);

/// <summary>
/// Alta de un usuario (RN-01 a RN-04, RN-07): cuenta en Supabase con el
/// documento como contraseña, y su perfil en Spotta.
/// </summary>
public sealed class CrearUsuario(SpottaDb db, ClienteAuthSupabase auth)
{
    public async Task<Guid> EjecutarAsync(CrearUsuarioSolicitud solicitud, Guid autorId, CancellationToken ct)
    {
        var autor = await PerfilActivo.CargarAsync(db, autorId, ct);
        return await CrearEnGimnasioAsync(solicitud, autor.GimnasioId, autor.Id, ct);
    }

    /// <summary>
    /// Sin autor sólo lo llama <c>sembrar-admin</c>: el primer administrador no
    /// tiene quién lo cree (RN-01).
    /// </summary>
    public async Task<Guid> CrearEnGimnasioAsync(
        CrearUsuarioSolicitud solicitud, Guid gimnasioId, Guid? autorId, CancellationToken ct)
    {
        var datos = DatosPersonales.Validar(solicitud.Nombre, solicitud.Apellidos, solicitud.Dni, solicitud.Email);

        if (await db.Perfiles.AnyAsync(p => p.GimnasioId == gimnasioId && p.Dni == datos.Dni, ct))
            throw ErroresUsuarios.DniDuplicado();

        Guid id;
        try
        {
            // La contraseña inicial es el documento (RN-04)
            id = await auth.CrearUsuarioAsync(datos.Email, datos.Dni, solicitud.Rol, ct);
        }
        catch (ErrorDeSupabase e) when (e.Codigo == "email_exists")
        {
            throw ErroresUsuarios.EmailDuplicado();
        }

        db.Perfiles.Add(new Perfil
        {
            Id = id,
            GimnasioId = gimnasioId,
            Nombre = datos.Nombre,
            Apellidos = datos.Apellidos,
            Dni = datos.Dni,
            Email = datos.Email,
            Rol = solicitud.Rol,
            ContrasenaCambiada = false,
            CreadoPor = autorId,
        });

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            // Una cuenta sin perfil puede iniciar sesión pero la API la rechaza
            // en todo: es un estado inválido y no se deja vivo.
            await Compensacion.DeshacerAsync(
                () => auth.BorrarUsuarioAsync(id, CancellationToken.None), ex,
                $"la cuenta {id} existe en Supabase sin perfil en Spotta");

            if (ViolacionDeUnicidad.Es(ex, PerfilConfiguracion.IndiceDniUnico))
                throw ErroresUsuarios.DniDuplicado();
            throw;
        }

        return id;
    }
}
