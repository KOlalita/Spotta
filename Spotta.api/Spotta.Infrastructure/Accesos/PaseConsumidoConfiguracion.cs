using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spotta.Domain.Accesos;

namespace Spotta.Infrastructure.Accesos;

internal sealed class PaseConsumidoConfiguracion : IEntityTypeConfiguration<PaseConsumido>
{
    public void Configure(EntityTypeBuilder<PaseConsumido> e)
    {
        e.HasKey(x => new { x.DispositivoId, x.Paso });
        e.Property(x => x.ConsumidoEn).HasDefaultValueSql("now()");
        e.HasOne<PaseDispositivo>().WithMany().HasForeignKey(x => x.DispositivoId);
    }
}
