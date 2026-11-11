using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spotta.Domain.Accesos;
using Spotta.Domain.Usuarios;

namespace Spotta.Infrastructure.Accesos;

internal sealed class PaseDispositivoConfiguracion : IEntityTypeConfiguration<PaseDispositivo>
{
    public void Configure(EntityTypeBuilder<PaseDispositivo> e)
    {
        e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
        e.Property(x => x.CreadoEn).HasDefaultValueSql("now()");
        e.HasIndex(x => x.UsuarioId)
            .HasDatabaseName("ix_pase_dispositivo_activo")
            .HasFilter("revocado_en IS NULL");
        e.HasOne(x => x.Usuario).WithMany().HasForeignKey(x => x.UsuarioId);
        e.HasOne<Perfil>().WithMany().HasForeignKey(x => x.RevocadoPor)
            .OnDelete(DeleteBehavior.Restrict);
        e.Ignore(x => x.EstaRevocado);
    }
}
