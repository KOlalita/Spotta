using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spotta.Domain.Notificaciones;
using Spotta.Domain.Usuarios;

namespace Spotta.Infrastructure.Notificaciones;

internal sealed class NotificacionLecturaConfiguracion : IEntityTypeConfiguration<NotificacionLectura>
{
    public void Configure(EntityTypeBuilder<NotificacionLectura> e)
    {
        e.HasKey(x => new { x.NotificacionId, x.UsuarioId });
        e.Property(x => x.LeidaEn).HasDefaultValueSql("now()");
        e.HasOne<Notificacion>().WithMany().HasForeignKey(x => x.NotificacionId);
        e.HasOne<Perfil>().WithMany().HasForeignKey(x => x.UsuarioId);
    }
}
