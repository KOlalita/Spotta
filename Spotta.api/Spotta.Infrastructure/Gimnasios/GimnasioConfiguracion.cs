using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Spotta.Domain.Gimnasios;

namespace Spotta.Infrastructure.Gimnasios;

internal sealed class GimnasioConfiguracion : IEntityTypeConfiguration<Gimnasio>
{
    public void Configure(EntityTypeBuilder<Gimnasio> e)
    {
        e.ToTable(t =>
        {
            t.HasCheckConstraint("ck_gimnasio_aforo", "aforo_maximo > 0");
            t.HasCheckConstraint("ck_gimnasio_ventana",
                "ventana_presencia_min BETWEEN 15 AND 480");
            t.HasCheckConstraint("ck_gimnasio_dias_racha",
                "dias_minimos_racha BETWEEN 1 AND 7");
        });
        e.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
        e.Property(x => x.Moneda).HasMaxLength(3);
        e.Property(x => x.CreadoEn).HasDefaultValueSql("now()");
    }
}
