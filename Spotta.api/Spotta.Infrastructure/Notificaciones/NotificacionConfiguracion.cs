using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spotta.Domain.Notificaciones;
using Spotta.Domain.Gimnasios;
using Spotta.Domain.Usuarios;

namespace Spotta.Infrastructure.Notificaciones;

internal sealed class NotificacionConfiguracion : IEntityTypeConfiguration<Notificacion>
{
    public void Configure(EntityTypeBuilder<Notificacion> e)
    {
        e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
        e.Property(x => x.Titulo).HasMaxLength(120);
        e.Property(x => x.EnviadaEn).HasDefaultValueSql("now()");
        e.HasOne<Gimnasio>().WithMany().HasForeignKey(x => x.GimnasioId);
        e.HasOne<Perfil>().WithMany().HasForeignKey(x => x.EnviadaPor)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
