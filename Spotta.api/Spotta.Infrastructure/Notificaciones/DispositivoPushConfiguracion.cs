using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spotta.Domain.Notificaciones;

namespace Spotta.Infrastructure.Notificaciones;

internal sealed class DispositivoPushConfiguracion : IEntityTypeConfiguration<DispositivoPush>
{
    public void Configure(EntityTypeBuilder<DispositivoPush> e)
    {
        e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
        e.Property(x => x.VistoEn).HasDefaultValueSql("now()");
        e.HasIndex(x => x.ExpoPushToken).IsUnique();
        e.HasOne(x => x.Usuario).WithMany().HasForeignKey(x => x.UsuarioId);
    }
}
