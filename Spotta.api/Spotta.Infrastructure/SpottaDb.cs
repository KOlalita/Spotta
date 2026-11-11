using Microsoft.EntityFrameworkCore;
using Spotta.Domain.Accesos;
using Spotta.Domain.Notificaciones;
using Spotta.Domain.Gimnasios;
using Spotta.Domain.Membresias;
using Spotta.Domain.Usuarios;

namespace Spotta.Infrastructure;

/// <summary>
/// El contexto no configura nada: cada entidad tiene su clase de configuración
/// dentro de la carpeta de su módulo, y EF las recoge solas. Aquí sólo quedan
/// los conjuntos y las dos pasadas que afectan al modelo entero.
/// </summary>
public sealed class SpottaDb(DbContextOptions<SpottaDb> options) : DbContext(options)
{
    public DbSet<Gimnasio> Gimnasios => Set<Gimnasio>();
    public DbSet<Perfil> Perfiles => Set<Perfil>();
    public DbSet<PlanMembresia> Planes => Set<PlanMembresia>();
    public DbSet<Membresia> Membresias => Set<Membresia>();
    public DbSet<AjusteMembresia> Ajustes => Set<AjusteMembresia>();
    public DbSet<Acceso> Accesos => Set<Acceso>();
    public DbSet<PaseDispositivo> PasesDispositivo => Set<PaseDispositivo>();
    public DbSet<PaseConsumido> PasesConsumidos => Set<PaseConsumido>();
    public DbSet<Notificacion> Notificaciones => Set<Notificacion>();
    public DbSet<NotificacionLectura> LecturasDeNotificacion => Set<NotificacionLectura>();
    public DbSet<DispositivoPush> DispositivosPush => Set<DispositivoPush>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.ApplyConfigurationsFromAssembly(typeof(SpottaDb).Assembly);

        ConvencionesDelEsquema.ProhibirBorradoEnCascada(b);
        ConvencionesDelEsquema.AplicarNombresEnMinusculaConGuiones(b);
    }
}
